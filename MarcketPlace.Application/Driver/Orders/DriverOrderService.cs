using MarcketPlace.Application.Driver.Orders.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Driver.Orders
{
    public class DriverOrderService : IDriverOrderService
    {
        private readonly AppDbContext _context;

        public DriverOrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<DriverOrderDto>> GetAvailableOrdersAsync(
            int driverUserId,
            CancellationToken cancellationToken = default)
        {
            await GetDriverIdAsync(driverUserId, cancellationToken);

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(x => x.OrderStores)
                    .ThenInclude(x => x.Store)
                .Where(x =>
                    x.Status == OrderStatus.Pending &&
                    x.DriverId == null &&
                    x.CancelledAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return orders.Select(MapDriverOrder).ToList();
        }

        public async Task<IReadOnlyList<DriverOrderDto>> GetMyOrdersAsync(
            int driverUserId,
            CancellationToken cancellationToken = default)
        {
            var driverId = await GetDriverIdAsync(driverUserId, cancellationToken);

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(x => x.OrderStores)
                    .ThenInclude(x => x.Store)
                .Where(x => x.DriverId == driverId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return orders.Select(MapDriverOrder).ToList();
        }

        public async Task<DriverOrderDto> AcceptOrderAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            if (orderId <= 0)
                throw new InvalidOperationException("رقم الطلب غير صالح.");

            var driverId = await GetDriverIdAsync(driverUserId, cancellationToken);
            var utcNow = DateTime.UtcNow;

            var affectedRows = await _context.Orders
                .Where(x =>
                    x.Id == orderId &&
                    x.Status == OrderStatus.Pending &&
                    x.DriverId == null &&
                    x.CancelledAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.DriverId, driverId)
                    .SetProperty(x => x.Status, OrderStatus.DriverAssigned)
                    .SetProperty(x => x.DriverAssignedAt, utcNow)
                    .SetProperty(x => x.UpdatedAt, utcNow),
                    cancellationToken);

            if (affectedRows == 0)
            {
                var currentOrder = await _context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

                if (currentOrder is null)
                    throw new KeyNotFoundException("الطلب غير موجود.");

                if (currentOrder.DriverId == driverId)
                    return await LoadDriverOrderDtoAsync(orderId, cancellationToken);

                throw new InvalidOperationException("تم حجز هذا الطلب من سائق آخر أو لم يعد متاحًا.");
            }

            return await LoadDriverOrderDtoAsync(orderId, cancellationToken);
        }

        public async Task<DriverOrderDto> PickUpOrderAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            if (orderId <= 0)
                throw new InvalidOperationException("رقم الطلب غير صالح.");

            var driverId = await GetDriverIdAsync(driverUserId, cancellationToken);
            var utcNow = DateTime.UtcNow;

            var affectedRows = await _context.Orders
                .Where(x =>
                    x.Id == orderId &&
                    x.DriverId == driverId &&
                    x.Status == OrderStatus.DriverAssigned &&
                    x.CancelledAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, OrderStatus.PickedUp)
                    .SetProperty(x => x.PickedUpAt, utcNow)
                    .SetProperty(x => x.UpdatedAt, utcNow),
                    cancellationToken);

            if (affectedRows == 0)
            {
                var currentOrder = await _context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

                if (currentOrder is null)
                    throw new KeyNotFoundException("الطلب غير موجود.");

                if (currentOrder.DriverId != driverId)
                    throw new InvalidOperationException("لا يمكنك استلام طلب لا يخصك.");

                if (currentOrder.Status == OrderStatus.PickedUp)
                    return await LoadDriverOrderDtoAsync(orderId, cancellationToken);

                throw new InvalidOperationException("لا يمكن استلام الطلب في حالته الحالية.");
            }

            return await LoadDriverOrderDtoAsync(orderId, cancellationToken);
        }

        public async Task<DriverOrderDto> DeliverOrderAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            if (orderId <= 0)
                throw new InvalidOperationException("رقم الطلب غير صالح.");

            var driverId = await GetDriverIdAsync(driverUserId, cancellationToken);
            var utcNow = DateTime.UtcNow;

            var affectedRows = await _context.Orders
                .Where(x =>
                    x.Id == orderId &&
                    x.DriverId == driverId &&
                    x.Status == OrderStatus.PickedUp &&
                    x.CancelledAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, OrderStatus.Delivered)
                    .SetProperty(x => x.DeliveredAt, utcNow)
                    .SetProperty(x => x.UpdatedAt, utcNow),
                    cancellationToken);

            if (affectedRows == 0)
            {
                var currentOrder = await _context.Orders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

                if (currentOrder is null)
                    throw new KeyNotFoundException("الطلب غير موجود.");

                if (currentOrder.DriverId != driverId)
                    throw new InvalidOperationException("لا يمكنك تسليم طلب لا يخصك.");

                if (currentOrder.Status == OrderStatus.Delivered)
                    return await LoadDriverOrderDtoAsync(orderId, cancellationToken);

                throw new InvalidOperationException("لا يمكن تسليم الطلب قبل استلامه.");
            }

            return await LoadDriverOrderDtoAsync(orderId, cancellationToken);
        }

        private async Task<int> GetDriverIdAsync(
            int driverUserId,
            CancellationToken cancellationToken)
        {
            var driver = await _context.Drivers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == driverUserId, cancellationToken);

            if (driver is null)
                throw new KeyNotFoundException("السائق غير موجود.");

            return driver.Id;
        }

        private async Task<DriverOrderDto> LoadDriverOrderDtoAsync(
            int orderId,
            CancellationToken cancellationToken)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(x => x.OrderStores)
                    .ThenInclude(x => x.Store)
                .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

            if (order is null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            return MapDriverOrder(order);
        }

        private static DriverOrderDto MapDriverOrder(Order order)
        {
            return new DriverOrderDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                AddressText = order.AddressText,
                Latitude = order.Latitude,
                Longitude = order.Longitude,
                CustomerNote = order.CustomerNote,
                Subtotal = order.Subtotal,
                DeliveryFee = order.DeliveryFee,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                DriverAssignedAt = order.DriverAssignedAt,
                PickedUpAt = order.PickedUpAt,
                DeliveredAt = order.DeliveredAt,
                Stores = order.OrderStores
                    .Select(x => new DriverOrderStoreDto
                    {
                        OrderStoreId = x.Id,
                        StoreId = x.StoreId,
                        StoreNameAr = x.Store?.NameAr ?? string.Empty,
                        StoreNameEn = x.Store?.NameEn ?? string.Empty,
                        StoreSubtotal = x.StoreSubtotal,
                        Status = x.Status,
                        ReadyAt = x.ReadyAt
                    })
                    .ToList()
            };
        }
    }
}