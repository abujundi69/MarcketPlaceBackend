using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Infrastructure.Data
{
    public static class DbInitializer
    {
        private const string SuperAdminPhone = "0599000000";
        private const string SuperAdminPassword = "0599000000@@";

        public static async Task EnsureSuperAdminExistsAsync(AppDbContext context, bool resetPasswordInDev = false, CancellationToken cancellationToken = default)
        {
            var user = await context.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == SuperAdminPhone, cancellationToken);

            var hasher = new PasswordHasher<User>();

            if (user is null)
            {
                user = new User
                {
                    FullName = "Super Admin",
                    PhoneNumber = SuperAdminPhone,
                    Role = UserRole.SuperAdmin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                user.PasswordHash = hasher.HashPassword(user, SuperAdminPassword);
                context.Users.Add(user);
            }
            else if (resetPasswordInDev)
            {
                user.PasswordHash = hasher.HashPassword(user, SuperAdminPassword);
                user.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
