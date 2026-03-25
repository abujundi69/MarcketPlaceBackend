using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarcketPlace.Application.Auth.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CustomerEntity = MarcketPlace.Domain.Entities.Customer;
using UserEntity = MarcketPlace.Domain.Entities.User;

namespace MarcketPlace.Application.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<UserEntity> _passwordHasher;
        private readonly ITwilioVerifyService _twilioVerifyService;

        private static readonly TimeSpan FirstResendDelay = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan NextResendDelay = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan OtpLifetime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan OtpCycleResetAfter = TimeSpan.FromDays(1);
        private static readonly TimeSpan ForgotPasswordResetWindow = TimeSpan.FromMinutes(10);

        public AuthService(
            AppDbContext context,
            IConfiguration configuration,
            IPasswordHasher<UserEntity> passwordHasher,
            ITwilioVerifyService twilioVerifyService)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            _twilioVerifyService = twilioVerifyService;
        }

        public async Task<LoginResultDto> CustomerRegisterAsync(
            CustomerRegisterRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var fullName = dto.FullName?.Trim();
            var phoneNumber = dto.PhoneNumber?.Trim();
            var password = dto.Password;
            var confirmPassword = dto.ConfirmPassword;

            if (string.IsNullOrWhiteSpace(fullName))
                throw new Exception("الاسم الكامل مطلوب.");

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new Exception("رقم الهاتف مطلوب.");

            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("كلمة المرور مطلوبة.");

            if (password != confirmPassword)
                throw new Exception("تأكيد كلمة المرور غير مطابق.");

            var phoneExists = await _context.Users
                .AnyAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);

            if (phoneExists)
                throw new Exception("رقم الهاتف مستخدم مسبقًا.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var now = DateTime.UtcNow;

            var user = new UserEntity
            {
                FullName = fullName!,
                PhoneNumber = phoneNumber!,
                Role = UserRole.Customer,
                IsActive = true,
                IsPhoneVerified = false,
                PhoneVerifiedAtUtc = null,
                CreatedAt = now
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            var customer = new CustomerEntity
            {
                UserId = user.Id,
                CreatedAt = now
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return await StartOtpChallengeAsync(
                user,
                OtpPurpose.RegisterCustomer,
                "تم إنشاء الحساب وإرسال رمز التحقق إلى رقم الهاتف.",
                cancellationToken);
        }

        public async Task<LoginResultDto> LoginAsync(
            LoginRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var phoneNumber = dto.PhoneNumber?.Trim();
            var password = dto.Password;

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new Exception("رقم الهاتف مطلوب.");

            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("كلمة المرور مطلوبة.");

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);

            if (user is null)
                throw new Exception("بيانات الدخول غير صحيحة.");

            if (!user.IsActive)
                throw new Exception("الحساب غير مفعل.");

            var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (verifyResult == PasswordVerificationResult.Failed)
                throw new Exception("بيانات الدخول غير صحيحة.");

            if (user.Role == UserRole.Customer && !user.IsPhoneVerified)
            {
                return await StartOtpChallengeAsync(
                    user,
                    OtpPurpose.LoginCustomer,
                    "تم إرسال رمز التحقق لتأكيد رقم الهاتف.",
                    cancellationToken);
            }

            return new LoginResultDto
            {
                RequiresOtp = false,
                Auth = BuildAuthResponse(user)
            };
        }

        public async Task<AuthResponseDto> VerifyCustomerOtpAsync(
            VerifyCustomerOtpRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto.OtpSessionId <= 0)
                throw new Exception("جلسة التحقق غير صالحة.");

            if (string.IsNullOrWhiteSpace(dto.Code))
                throw new Exception("رمز التحقق مطلوب.");

            var otpSession = await _context.OtpCodes
                .FirstOrDefaultAsync(x => x.Id == dto.OtpSessionId, cancellationToken);

            if (otpSession is null)
                throw new Exception("جلسة التحقق غير موجودة.");

            if (otpSession.Purpose != OtpPurpose.RegisterCustomer &&
                otpSession.Purpose != OtpPurpose.LoginCustomer)
                throw new Exception("نوع جلسة التحقق غير صحيح.");

            if (otpSession.IsUsed)
                throw new Exception("تم استخدام جلسة التحقق من قبل.");

            if (otpSession.ExpiresAt < DateTime.UtcNow)
                throw new Exception("انتهت صلاحية جلسة التحقق.");

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == otpSession.UserId, cancellationToken);

            if (user is null)
                throw new Exception("المستخدم غير موجود.");

            if (!user.IsActive)
                throw new Exception("الحساب غير مفعل.");

            if (user.Role != UserRole.Customer)
                throw new Exception("التحقق بالرمز متاح للعملاء فقط.");

            var approved = await _twilioVerifyService.VerifyCodeAsync(otpSession.PhoneNumber, dto.Code);
            if (!approved)
                throw new Exception("رمز التحقق غير صحيح أو منتهي.");

            var now = DateTime.UtcNow;

            if (!user.IsPhoneVerified)
            {
                user.IsPhoneVerified = true;
                user.PhoneVerifiedAtUtc = now;
            }

            otpSession.IsUsed = true;
            otpSession.VerifiedAt = now;

            var otherPendingSessions = await _context.OtpCodes
                .Where(x => x.UserId == user.Id && !x.IsUsed && x.Id != otpSession.Id)
                .ToListAsync(cancellationToken);

            foreach (var session in otherPendingSessions)
                session.IsUsed = true;

            await _context.SaveChangesAsync(cancellationToken);

            return BuildAuthResponse(user);
        }

        public async Task<LoginResultDto> ForgotPasswordAsync(
            ForgotPasswordRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var phoneNumber = dto.PhoneNumber?.Trim();

            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new Exception("رقم الهاتف مطلوب.");

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);

            if (user is null)
                throw new Exception("لا يوجد حساب مرتبط بهذا الرقم.");

            if (!user.IsActive)
                throw new Exception("الحساب غير مفعل.");

            return await StartOtpChallengeAsync(
                user,
                OtpPurpose.ForgotPassword,
                "تم إرسال رمز التحقق لإعادة تعيين كلمة المرور.",
                cancellationToken);
        }

        public async Task<VerifyForgotPasswordOtpResultDto> VerifyForgotPasswordOtpAsync(
            VerifyForgotPasswordOtpRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto.OtpSessionId <= 0)
                throw new Exception("جلسة التحقق غير صالحة.");

            if (string.IsNullOrWhiteSpace(dto.Code))
                throw new Exception("رمز التحقق مطلوب.");

            var now = DateTime.UtcNow;

            var otpSession = await _context.OtpCodes
                .FirstOrDefaultAsync(x => x.Id == dto.OtpSessionId, cancellationToken);

            if (otpSession is null)
                throw new Exception("جلسة التحقق غير موجودة.");

            if (otpSession.Purpose != OtpPurpose.ForgotPassword)
                throw new Exception("هذه الجلسة ليست مخصصة لنسيت كلمة المرور.");

            if (otpSession.IsUsed)
                throw new Exception("تم استخدام جلسة التحقق من قبل.");

            if (otpSession.VerifiedAt.HasValue && otpSession.ExpiresAt > now)
            {
                return new VerifyForgotPasswordOtpResultDto
                {
                    CanResetPassword = true,
                    OtpSessionId = otpSession.Id,
                    Message = "تم التحقق من الرمز مسبقًا. يمكنك الآن إدخال كلمة المرور الجديدة."
                };
            }

            if (otpSession.ExpiresAt < now)
                throw new Exception("انتهت صلاحية جلسة التحقق.");

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == otpSession.UserId, cancellationToken);

            if (user is null)
                throw new Exception("المستخدم غير موجود.");

            if (!user.IsActive)
                throw new Exception("الحساب غير مفعل.");

            var approved = await _twilioVerifyService.VerifyCodeAsync(otpSession.PhoneNumber, dto.Code);
            if (!approved)
                throw new Exception("رمز التحقق غير صحيح أو منتهي.");

            otpSession.VerifiedAt = now;
            otpSession.ExpiresAt = now.Add(ForgotPasswordResetWindow);

            if (!user.IsPhoneVerified)
            {
                user.IsPhoneVerified = true;
                user.PhoneVerifiedAtUtc = now;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new VerifyForgotPasswordOtpResultDto
            {
                CanResetPassword = true,
                OtpSessionId = otpSession.Id,
                Message = "تم التحقق من الرمز بنجاح. يمكنك الآن إدخال كلمة المرور الجديدة."
            };
        }

        public async Task<MessageResultDto> ResetForgotPasswordAsync(
            ResetForgotPasswordRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto.OtpSessionId <= 0)
                throw new Exception("جلسة التحقق غير صالحة.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new Exception("كلمة المرور الجديدة مطلوبة.");

            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new Exception("تأكيد كلمة المرور غير مطابق.");

            var now = DateTime.UtcNow;

            var otpSession = await _context.OtpCodes
                .FirstOrDefaultAsync(x => x.Id == dto.OtpSessionId, cancellationToken);

            if (otpSession is null)
                throw new Exception("جلسة التحقق غير موجودة.");

            if (otpSession.Purpose != OtpPurpose.ForgotPassword)
                throw new Exception("هذه الجلسة ليست مخصصة لإعادة تعيين كلمة المرور.");

            if (otpSession.IsUsed)
                throw new Exception("تم استخدام جلسة التحقق من قبل.");

            if (!otpSession.VerifiedAt.HasValue)
                throw new Exception("يجب التحقق من رمز OTP أولاً.");

            if (otpSession.ExpiresAt < now)
                throw new Exception("انتهت صلاحية جلسة إعادة تعيين كلمة المرور.");

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == otpSession.UserId, cancellationToken);

            if (user is null)
                throw new Exception("المستخدم غير موجود.");

            if (!user.IsActive)
                throw new Exception("الحساب غير مفعل.");

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);

            if (!user.IsPhoneVerified)
            {
                user.IsPhoneVerified = true;
                user.PhoneVerifiedAtUtc = now;
            }

            otpSession.IsUsed = true;

            var otherPendingSessions = await _context.OtpCodes
                .Where(x => x.UserId == user.Id && !x.IsUsed && x.Id != otpSession.Id)
                .ToListAsync(cancellationToken);

            foreach (var session in otherPendingSessions)
                session.IsUsed = true;

            await _context.SaveChangesAsync(cancellationToken);

            return new MessageResultDto
            {
                Message = "تم تغيير كلمة المرور بنجاح. يمكنك الآن تسجيل الدخول بكلمة المرور الجديدة."
            };
        }

        private async Task<LoginResultDto> StartOtpChallengeAsync(
            UserEntity user,
            OtpPurpose purpose,
            string successMessage,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var otpState = await GetOtpChallengeStateAsync(user.Id, now, cancellationToken);

            if (otpState.IsBlocked)
            {
                var existingSessionId = otpState.ActiveSession?.Id ?? otpState.LastSession?.Id ?? 0;

                return new LoginResultDto
                {
                    RequiresOtp = true,
                    OtpSessionId = existingSessionId,
                    Message = $"تم إرسال رمز تحقق مسبقًا. استخدم الرمز الحالي أو أعد الطلب بعد {FormatRemainingTime(otpState.CanSendAt - now)}."
                };
            }

            await _twilioVerifyService.SendCodeAsync(user.PhoneNumber);

            var oldSessions = await _context.OtpCodes
                .Where(x => x.UserId == user.Id && !x.IsUsed)
                .ToListAsync(cancellationToken);

            foreach (var session in oldSessions)
                session.IsUsed = true;

            var otpSession = new OtpCode
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                Purpose = purpose,
                CreatedAt = now,
                ExpiresAt = now.Add(OtpLifetime),
                IsUsed = false,
                VerifiedAt = null
            };

            _context.OtpCodes.Add(otpSession);
            await _context.SaveChangesAsync(cancellationToken);

            return new LoginResultDto
            {
                RequiresOtp = true,
                OtpSessionId = otpSession.Id,
                Message = $"{successMessage} يمكنك طلب رمز جديد بعد {FormatRemainingTime(otpState.CooldownAfterThisSend)}."
            };
        }

        private async Task<OtpChallengeState> GetOtpChallengeStateAsync(
            int userId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var history = await _context.OtpCodes
                .Where(x => x.UserId == userId && x.CreatedAt >= now.AddDays(-2))
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            var activeSession = history.LastOrDefault(x => !x.IsUsed && x.ExpiresAt > now);
            var currentCycleSessions = GetCurrentCycleSessions(history, now);

            if (currentCycleSessions.Count == 0)
            {
                return new OtpChallengeState
                {
                    IsBlocked = false,
                    CanSendAt = now,
                    ActiveSession = activeSession,
                    LastSession = history.LastOrDefault(),
                    CooldownAfterThisSend = FirstResendDelay
                };
            }

            var lastSessionInCycle = currentCycleSessions[^1];
            var resendDelay = currentCycleSessions.Count == 1
                ? FirstResendDelay
                : NextResendDelay;

            var canSendAt = lastSessionInCycle.CreatedAt.Add(resendDelay);

            return new OtpChallengeState
            {
                IsBlocked = now < canSendAt,
                CanSendAt = canSendAt,
                ActiveSession = activeSession,
                LastSession = lastSessionInCycle,
                CooldownAfterThisSend = currentCycleSessions.Count == 0
                    ? FirstResendDelay
                    : NextResendDelay
            };
        }

        private static List<OtpCode> GetCurrentCycleSessions(List<OtpCode> history, DateTime now)
        {
            if (history.Count == 0)
                return new List<OtpCode>();

            List<OtpCode> currentCycle = new();
            DateTime? cycleStart = null;

            foreach (var session in history)
            {
                if (cycleStart is null || session.CreatedAt >= cycleStart.Value.Add(OtpCycleResetAfter))
                {
                    cycleStart = session.CreatedAt;
                    currentCycle = new List<OtpCode> { session };
                }
                else
                {
                    currentCycle.Add(session);
                }
            }

            if (cycleStart is null)
                return new List<OtpCode>();

            if (now >= cycleStart.Value.Add(OtpCycleResetAfter))
                return new List<OtpCode>();

            return currentCycle;
        }

        private static string FormatRemainingTime(TimeSpan remaining)
        {
            if (remaining <= TimeSpan.Zero)
                return "الآن";

            var totalMinutes = (int)Math.Ceiling(remaining.TotalMinutes);
            if (totalMinutes >= 1)
            {
                if (totalMinutes == 1)
                    return "دقيقة واحدة";

                if (totalMinutes == 2)
                    return "دقيقتين";

                if (totalMinutes <= 10)
                    return $"{totalMinutes} دقائق";

                return $"{totalMinutes} دقيقة";
            }

            var totalSeconds = Math.Max(1, (int)Math.Ceiling(remaining.TotalSeconds));
            return $"{totalSeconds} ثانية";
        }

        private AuthResponseDto BuildAuthResponse(UserEntity user)
        {
            var expiresMinutes = int.Parse(_configuration["Jwt:ExpiresMinutes"]!);
            var expiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes);
            var token = GenerateJwtToken(user, expiresAt);

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                UserId = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString()
            };
        }

        private string GenerateJwtToken(UserEntity user, DateTime expiresAt)
        {
            var key = _configuration["Jwt:Key"]!;
            var issuer = _configuration["Jwt:Issuer"]!;
            var audience = _configuration["Jwt:Audience"]!;

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jwtToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        private sealed class OtpChallengeState
        {
            public bool IsBlocked { get; init; }
            public DateTime CanSendAt { get; init; }
            public OtpCode? ActiveSession { get; init; }
            public OtpCode? LastSession { get; init; }
            public TimeSpan CooldownAfterThisSend { get; init; }
        }
    }
}