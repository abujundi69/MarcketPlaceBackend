using MarcketPlace.Application.Admin.Orders.Dtos;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.Orders
{
    public class AdminOrderService : IAdminOrderService
    {
        private readonly AppDbContext _context;

        public AdminOrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<AdminOrderListItemDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.User)
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .Include(x => x.OrderStores)
                    .ThenInclude(x => x.Store)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return orders.Select(x => new AdminOrderListItemDto
            {
                Id = x.Id,
                OrderNumber = x.OrderNumber,
                CustomerName = x.Customer.User.FullName,
                StoresText = string.Join("، ", x.OrderStores
                    .Select(s => s.Store.NameAr)
                    .Distinct()),
                StatusText = GetStatusText(x.Status),
                DriverName = x.Driver != null ? x.Driver.User.FullName : null,
                TotalAmount = x.TotalAmount,
                CreatedAt = x.CreatedAt,
                CreatedAtText = x.CreatedAt.ToString("yyyy-MM-dd hh:mm tt")
            }).ToList();
        }

        private static string GetStatusText(OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Pending",
                OrderStatus.DriverAssigned => "Driver Assigned",
                OrderStatus.PickedUp => "Picked Up",
                OrderStatus.Delivered => "Delivered",
                OrderStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }
    }
}