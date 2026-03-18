using MarcketPlace.Application.Admin.Dashboard.Dtos;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.Dashboard
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly AppDbContext _context;

        public AdminDashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SuperAdminDashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
        {
            var totalUsersCount = await _context.Users
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var activeVendorsCount = await _context.Vendors
                .AsNoTracking()
                .CountAsync(x => x.User.IsActive, cancellationToken);

            var driversCount = await _context.Drivers
                .AsNoTracking()
                .CountAsync(cancellationToken);

            var customerOrdersCount = await _context.Orders
                .AsNoTracking()
                .CountAsync(cancellationToken);

            return new SuperAdminDashboardStatsDto
            {
                TotalUsersCount = totalUsersCount,
                ActiveVendorsCount = activeVendorsCount,
                DriversCount = driversCount,
                CustomerOrdersCount = customerOrdersCount
            };
        }
    }
}