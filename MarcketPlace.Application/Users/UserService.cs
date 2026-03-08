using MarcketPlace.Application.Users.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Users
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(
            AppDbContext context,
            IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<IReadOnlyList<UserListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _context.Users
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new UserListItemDto
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    PhoneNumber = x.PhoneNumber,
                    Role = x.Role,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync(cancellationToken);

            foreach (var user in users)
            {
                FillComputedFields(user);
            }

            return users;
        }

        public async Task<UserListItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new UserListItemDto
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    PhoneNumber = x.PhoneNumber,
                    Role = x.Role,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
                return null;

            FillComputedFields(user);

            return user;
        }

        public async Task ChangePasswordByAdminAsync(
    int userId,
    AdminChangeUserPasswordDto dto,
    CancellationToken cancellationToken = default)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

            if (user is null)
                throw new KeyNotFoundException("المستخدم غير موجود.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                throw new ArgumentException("كلمة السر الجديدة مطلوبة.");

            if (dto.NewPassword.Length < 6)
                throw new ArgumentException("كلمة السر يجب أن تكون 6 أحرف على الأقل.");

            if (dto.NewPassword != dto.ConfirmPassword)
                throw new ArgumentException("تأكيد كلمة السر غير مطابق.");

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        private static void FillComputedFields(UserListItemDto user)
        {
            user.RoleName = user.Role switch
            {
                UserRole.SuperAdmin => "Super Admin",
                UserRole.Vendor => "Vendor",
                UserRole.Driver => "Driver",
                UserRole.Customer => "Customer",
                _ => "Unknown"
            };

            user.AccountStatus = user.IsActive ? "Active" : "Inactive";
            user.CreatedAtText = user.CreatedAt.ToString("yyyy-MM-dd hh:mm tt");
        }
    }
}