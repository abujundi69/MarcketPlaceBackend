using MarcketPlace.Application.Shared.Orders.Dtos;
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

        public async Task<IReadOnlyList<OrderListItemDto>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            var orders = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.User)
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .Include(x => x.DeliveryZone)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return orders.Select(MapListItem).ToList();
        }

        public async Task<OrderDetailsDto> GetByIdAsync(
            int orderId,
            CancellationToken cancellationToken = default)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.User)
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .Include(x => x.DeliveryZone)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == orderId, cancellationToken);

            if (order is null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            return new OrderDetailsDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,

                CustomerId = order.CustomerId,
                CustomerName = order.Customer.User.FullName,
                CustomerPhoneNumber = order.Customer.User.PhoneNumber,

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

        private static OrderListItemDto MapListItem(Domain.Entities.Order order)
        {
            return new OrderListItemDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,

                CustomerName = order.Customer.User.FullName,
                CustomerPhoneNumber = order.Customer.User.PhoneNumber,

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
    }
}