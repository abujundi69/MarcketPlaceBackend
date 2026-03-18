using MarcketPlace.Application.Account.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Account
{
    public class MyAccountService : IMyAccountService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public MyAccountService(
            AppDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<MyProfileDto> GetMyProfileAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user is null)
                throw new KeyNotFoundException("المستخدم غير موجود.");

            return new MyProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<MyProfileDto> UpdateMyProfileAsync(int userId, UpdateMyProfileDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user is null)
                throw new KeyNotFoundException("المستخدم غير موجود.");

            if (!user.IsActive)
                throw new InvalidOperationException("الحساب غير نشط.");

            var fullName = dto.FullName?.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                throw new InvalidOperationException("الاسم الكامل مطلوب.");

            if (fullName.Length > 200)
                throw new InvalidOperationException("الاسم الكامل يجب ألا يتجاوز 200 حرف.");

            user.FullName = fullName;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return new MyProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task ChangePasswordAsync(int userId, ChangeMyPasswordDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user is null)
                throw new KeyNotFoundException("المستخدم غير موجود.");

            if (!user.IsActive)
                throw new InvalidOperationException("الحساب غير نشط.");

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                throw new InvalidOperationException("كلمة المرور الحالية مطلوبة.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new InvalidOperationException("كلمة المرور الجديدة مطلوبة.");

            if (string.IsNullOrWhiteSpace(dto.ConfirmNewPassword))
                throw new InvalidOperationException("تأكيد كلمة المرور الجديدة مطلوب.");

            if (dto.NewPassword != dto.ConfirmNewPassword)
                throw new InvalidOperationException("تأكيد كلمة المرور غير مطابق.");

            if (dto.NewPassword.Length < 6)
                throw new InvalidOperationException("كلمة المرور الجديدة يجب أن تكون 6 أحرف على الأقل.");

            var verifyResult = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.CurrentPassword);

            if (verifyResult == PasswordVerificationResult.Failed)
                throw new InvalidOperationException("كلمة المرور الحالية غير صحيحة.");

            if (dto.CurrentPassword == dto.NewPassword)
                throw new InvalidOperationException("كلمة المرور الجديدة يجب أن تكون مختلفة عن الحالية.");

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}