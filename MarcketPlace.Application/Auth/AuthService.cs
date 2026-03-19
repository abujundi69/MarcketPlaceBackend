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

            return await StartCustomerOtpChallengeAsync(
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
                return await StartCustomerOtpChallengeAsync(
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

            await _context.SaveChangesAsync(cancellationToken);

            return BuildAuthResponse(user);
        }

        private async Task<LoginResultDto> StartCustomerOtpChallengeAsync(
            UserEntity user,
            OtpPurpose purpose,
            string message,
            CancellationToken cancellationToken)
        {
            if (user.Role != UserRole.Customer)
                throw new Exception("OTP متاح للعملاء فقط.");

            await _twilioVerifyService.SendCodeAsync(user.PhoneNumber);

            var oldSessions = await _context.OtpCodes
                .Where(x => x.UserId == user.Id && !x.IsUsed)
                .ToListAsync(cancellationToken);

            foreach (var session in oldSessions)
                session.IsUsed = true;

            var now = DateTime.UtcNow;

            var otpSession = new OtpCode
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                Purpose = purpose,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(10),
                IsUsed = false
            };

            _context.OtpCodes.Add(otpSession);
            await _context.SaveChangesAsync(cancellationToken);

            return new LoginResultDto
            {
                RequiresOtp = true,
                OtpSessionId = otpSession.Id,
                Message = message
            };
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
    }
}