using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarcketPlace.Application.Auth.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MarcketPlace.Application.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ITwilioVerifyService _twilioVerifyService;

        public AuthService(
            AppDbContext context,
            IConfiguration configuration,
            IPasswordHasher<User> passwordHasher,
            ITwilioVerifyService twilioVerifyService)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            _twilioVerifyService = twilioVerifyService;
        }

        public async Task<LoginResultDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
        {
            var phoneNumber = dto.PhoneNumber?.Trim();
            var password = dto.Password?.Trim();

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

            if (!user.IsPhoneVerified)
            {
                await _twilioVerifyService.SendCodeAsync(user.PhoneNumber);

                var oldSessions = await _context.OtpCodes
                    .Where(x => x.UserId == user.Id && !x.IsUsed)
                    .ToListAsync(cancellationToken);

                foreach (var s in oldSessions)
                    s.IsUsed = true;

                var otpSession = new OtpCode
                {
                    UserId = user.Id,
                    PhoneNumber = user.PhoneNumber,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    IsUsed = false
                };

                _context.OtpCodes.Add(otpSession);
                await _context.SaveChangesAsync(cancellationToken);

                return new LoginResultDto
                {
                    RequiresOtp = true,
                    OtpSessionId = otpSession.Id,
                    Message = "تم إرسال رمز التحقق إلى رقم الهاتف."
                };
            }

            return new LoginResultDto
            {
                RequiresOtp = false,
                Auth = BuildAuthResponse(user)
            };
        }

        public async Task<AuthResponseDto> VerifyFirstLoginOtpAsync(
            VerifyFirstLoginOtpRequestDto dto,
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

            var approved = await _twilioVerifyService.VerifyCodeAsync(otpSession.PhoneNumber, dto.Code);
            if (!approved)
                throw new Exception("رمز التحقق غير صحيح أو منتهي.");

            user.IsPhoneVerified = true;
            user.PhoneVerifiedAtUtc = DateTime.UtcNow;

            otpSession.IsUsed = true;

            await _context.SaveChangesAsync(cancellationToken);

            return BuildAuthResponse(user);
        }

        private AuthResponseDto BuildAuthResponse(User user)
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

        private string GenerateJwtToken(User user, DateTime expiresAt)
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