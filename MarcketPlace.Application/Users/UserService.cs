using MarcketPlace.Application.Users.Dtos;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Users
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
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

            return users;
        }
    }
}