using MarcketPlace.Application.Shared.Orders.Dtos;
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

        public async Task<IReadOnlyList<OrderListItemDto>> GetAvailableOrdersAsync(
            int driverUserId,
            CancellationToken cancellationToken = default)
        {
            await GetDriverAsync(driverUserId, cancellationToken);

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.User)
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .Include(x => x.DeliveryZone)
                .Where(x =>
                    x.Status == OrderStatus.Pending &&
                    x.DriverId == null &&
                    x.CancelledAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return orders.Select(MapListItem).ToList();
        }

        public async Task<IReadOnlyList<OrderListItemDto>> GetMyOrdersAsync(
            int driverUserId,
            CancellationToken cancellationToken = default)
        {
            var driver = await GetDriverAsync(driverUserId, cancellationToken);

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.User)
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .Include(x => x.DeliveryZone)
                .Where(x => x.DriverId == driver.Id && x.CancelledAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return orders.Select(MapListItem).ToList();
        }

        public async Task<OrderDetailsDto> GetByIdAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            var driver = await GetDriverAsync(driverUserId, cancellationToken);

            var order = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.User)
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .Include(x => x.DeliveryZone)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x =>
                    x.Id == orderId &&
                    x.CancelledAt == null &&
                    (x.DriverId == null || x.DriverId == driver.Id),
                    cancellationToken);

            if (order is null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            return MapDetails(order);
        }

        public async Task<OrderDetailsDto> AcceptOrderAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            var driver = await GetDriverAsync(driverUserId, cancellationToken);
            var now = DateTime.UtcNow;

            var affectedRows = await _context.Orders
                .Where(x =>
                    x.Id == orderId &&
                    x.DriverId == null &&
                    x.Status == OrderStatus.Pending &&
                    x.CancelledAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.DriverId, driver.Id)
                    .SetProperty(x => x.Status, OrderStatus.DriverAssigned)
                    .SetProperty(x => x.DriverAssignedAt, now)
                    .SetProperty(x => x.UpdatedAt, now),
                    cancellationToken);

            if (affectedRows == 0)
            {
                var exists = await _context.Orders
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == orderId, cancellationToken);

                if (!exists)
                    throw new KeyNotFoundException("الطلب غير موجود.");

                throw new InvalidOperationException("الطلب لم يعد متاحًا للمندوب.");
            }

            var order = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.User)
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .Include(x => x.DeliveryZone)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == orderId && x.DriverId == driver.Id, cancellationToken);

            if (order is null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            return MapDetails(order);
        }

        public async Task<OrderDetailsDto> MarkPickedUpAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            var driver = await GetDriverAsync(driverUserId, cancellationToken);

            var order = await _context.Orders
                .Include(x => x.Customer)
                    .ThenInclude(x => x.User)
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .Include(x => x.DeliveryZone)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

            if (order is null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            if (order.DriverId != driver.Id)
                throw new InvalidOperationException("هذا الطلب ليس تابعًا لهذا المندوب.");

            if (order.CancelledAt.HasValue || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("الطلب ملغي.");

            if (order.Status != OrderStatus.DriverAssigned)
                throw new InvalidOperationException("لا يمكن تأكيد الاستلام إلا بعد تعيين الطلب للمندوب.");

            order.Status = OrderStatus.PickedUp;
            order.PickedUpAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return MapDetails(order);
        }

        public async Task<OrderDetailsDto> MarkDeliveredAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            var driver = await GetDriverAsync(driverUserId, cancellationToken);

            var order = await _context.Orders
                .Include(x => x.Customer)
                    .ThenInclude(x => x.User)
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .Include(x => x.DeliveryZone)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

            if (order is null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            if (order.DriverId != driver.Id)
                throw new InvalidOperationException("هذا الطلب ليس تابعًا لهذا المندوب.");

            if (order.CancelledAt.HasValue || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("الطلب ملغي.");

            if (order.Status != OrderStatus.PickedUp)
                throw new InvalidOperationException("لا يمكن تسليم الطلب قبل تأكيد استلامه.");

            order.Status = OrderStatus.Delivered;
            order.DeliveredAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return MapDetails(order);
        }

        public async Task CancelAssignmentAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            var driver = await GetDriverAsync(driverUserId, cancellationToken);

            var affectedRows = await _context.Orders
                .Where(x =>
                    x.Id == orderId &&
                    x.DriverId == driver.Id &&
                    x.Status == OrderStatus.DriverAssigned &&
                    x.CancelledAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.DriverId, (int?)null)
                    .SetProperty(x => x.Status, OrderStatus.Pending)
                    .SetProperty(x => x.DriverAssignedAt, (DateTime?)null)
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow),
                    cancellationToken);

            if (affectedRows == 0)
            {
                var exists = await _context.Orders
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == orderId, cancellationToken);

                if (!exists)
                    throw new KeyNotFoundException("الطلب غير موجود.");

                throw new InvalidOperationException("لا يمكن إلغاء تولي هذا الطلب.");
            }
        }

        private async Task<Domain.Entities.Driver> GetDriverAsync(
            int driverUserId,
            CancellationToken cancellationToken)
        {
            var driver = await _context.Drivers
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.UserId == driverUserId, cancellationToken);

            if (driver is null)
                throw new KeyNotFoundException("المندوب غير موجود.");

            return driver;
        }

        private static OrderListItemDto MapListItem(Domain.Entities.Order order)
        {
            return new OrderListItemDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,

                CustomerName = order.Customer.User.FullName,
                CustomerPhoneNumber = order.Customer.User.PhoneNumber,
                CustomerWhatsAppNumber = order.CustomerWhatsAppNumber,

                DriverName = order.Driver?.User.FullName,
                DriverPhoneNumber = order.Driver?.User.PhoneNumber,

                Subtotal = order.Subtotal,
                DeliveryFee = order.DeliveryFee,
                TotalAmount = order.TotalAmount,

                CreatedAt = order.CreatedAt,
                DriverAssignedAt = order.DriverAssignedAt,
                PickedUpAt = order.PickedUpAt,
                DeliveredAt = order.DeliveredAt,

                Pickup = new OrderPickupDto
                {
                    PickupLocationNameAr = order.PickupLocationNameAr,
                    PickupLocationNameEn = order.PickupLocationNameEn,
                    PickupAddressText = order.PickupAddressText,
                    PickupLatitude = order.PickupLatitude,
                    PickupLongitude = order.PickupLongitude
                },

                Destination = new OrderDestinationDto
                {
                    DeliveryZoneId = order.DeliveryZoneId,
                    DeliveryZoneNameAr = order.DeliveryZone.NameAr,
                    DeliveryZoneNameEn = order.DeliveryZone.NameEn,
                    AddressText = order.AddressText,
                    Latitude = order.Latitude,
                    Longitude = order.Longitude
                }
            };
        }

        private static OrderDetailsDto MapDetails(Domain.Entities.Order order)
        {
            return new OrderDetailsDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,

                CustomerId = order.CustomerId,
                CustomerName = order.Customer.User.FullName,
                CustomerPhoneNumber = order.Customer.User.PhoneNumber,
                CustomerWhatsAppNumber = order.CustomerWhatsAppNumber,

                DriverId = order.DriverId,
                DriverName = order.Driver?.User.FullName,
                DriverPhoneNumber = order.Driver?.User.PhoneNumber,

                CustomerNote = order.CustomerNote,

                Subtotal = order.Subtotal,
                DeliveryFee = order.DeliveryFee,
                TotalAmount = order.TotalAmount,

                CreatedAt = order.CreatedAt,
                DriverAssignedAt = order.DriverAssignedAt,
                PickedUpAt = order.PickedUpAt,
                DeliveredAt = order.DeliveredAt,
                CancelledAt = order.CancelledAt,
                CancelReason = order.CancelReason,

                Pickup = new OrderPickupDto
                {
                    PickupLocationNameAr = order.PickupLocationNameAr,
                    PickupLocationNameEn = order.PickupLocationNameEn,
                    PickupAddressText = order.PickupAddressText,
                    PickupLatitude = order.PickupLatitude,
                    PickupLongitude = order.PickupLongitude
                },

                Destination = new OrderDestinationDto
                {
                    DeliveryZoneId = order.DeliveryZoneId,
                    DeliveryZoneNameAr = order.DeliveryZone.NameAr,
                    DeliveryZoneNameEn = order.DeliveryZone.NameEn,
                    AddressText = order.AddressText,
                    Latitude = order.Latitude,
                    Longitude = order.Longitude
                },

                Items = order.Items
                    .OrderBy(x => x.Id)
                    .Select(x => new OrderItemViewDto
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,
                        ProductVariantId = x.ProductVariantId,
                        ProductNameAr = x.ProductNameAr,
                        ProductNameEn = x.ProductNameEn,
                        ProductImage = x.ProductImage,
                        VariantNameAr = x.VariantNameAr,
                        VariantNameEn = x.VariantNameEn,
                        UnitSymbol = x.UnitSymbol,
                        UnitPrice = x.UnitPrice,
                        Quantity = x.Quantity,
                        RequestedAmount = x.RequestedAmount,
                        PurchaseInputMode = x.PurchaseInputMode,
                        LineTotal = x.LineTotal
                    })
                    .ToList()
            };
        }
    }
}