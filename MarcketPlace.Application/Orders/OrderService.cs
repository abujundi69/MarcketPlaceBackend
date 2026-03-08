using MarcketPlace.Application.Orders.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Orders
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CheckoutOrderResultDto> CheckoutFromCartAsync(
            CheckoutOrderDto dto,
            CancellationToken cancellationToken = default)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.CustomerId, cancellationToken);

            if (customer is null)
                throw new Exception("العميل غير موجود.");

            var deliveryZone = await _context.DeliveryZones
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.DeliveryZoneId, cancellationToken);

            if (deliveryZone is null)
                throw new Exception("منطقة التوصيل غير موجودة.");

            var cartItems = await _context.CartItems
                .Include(x => x.Product)
                    .ThenInclude(x => x.Store)
                .Where(x => x.CustomerId == dto.CustomerId)
                .ToListAsync(cancellationToken);

            if (!cartItems.Any())
                throw new Exception("السلة فارغة.");

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Quantity <= 0)
                    throw new Exception("يوجد منتج بكمية غير صالحة.");

                if (cartItem.Product is null)
                    throw new Exception("يوجد منتج غير صالح في السلة.");

                if (cartItem.Product.StoreId is null || cartItem.Product.Store is null)
                    throw new Exception($"المنتج {cartItem.Product.NameAr} غير مربوط بمتجر.");

                if (!cartItem.Product.Store.IsActive)
                    throw new Exception($"المتجر {cartItem.Product.Store.NameAr} غير نشط.");

                if (cartItem.Product.StockQuantity < cartItem.Quantity)
                    throw new Exception($"الكمية غير متوفرة للمنتج {cartItem.Product.NameAr}.");
            }

            var subtotal = cartItems.Sum(x => x.Product.Price * x.Quantity);
            var deliveryFee = deliveryZone.DeliveryFee;
            var totalAmount = subtotal + deliveryFee;

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var order = new Order
            {
                OrderNumber = "TEMP",
                CustomerId = dto.CustomerId,
                DriverId = null,
                DeliveryZoneId = dto.DeliveryZoneId,
                Status = OrderStatus.Pending,
                AddressText = dto.AddressText,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CustomerNote = dto.CustomerNote,
                Subtotal = subtotal,
                DeliveryFee = deliveryFee,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            order.OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{order.Id:D6}";
            order.UpdatedAt = DateTime.UtcNow;

            var itemsGroupedByStore = cartItems.GroupBy(x => x.Product.StoreId!.Value);

            foreach (var storeGroup in itemsGroupedByStore)
            {
                var orderStore = new OrderStore
                {
                    OrderId = order.Id,
                    StoreId = storeGroup.Key,
                    Status = OrderStoreStatus.Pending,
                    StoreSubtotal = storeGroup.Sum(x => x.Product.Price * x.Quantity),
                    CreatedAt = DateTime.UtcNow
                };

                _context.OrderStores.Add(orderStore);
                await _context.SaveChangesAsync(cancellationToken);

                foreach (var cartItem in storeGroup)
                {
                    var product = cartItem.Product;

                    var orderItem = new OrderItem
                    {
                        OrderStoreId = orderStore.Id,
                        ProductId = product.Id,
                        ProductNameAr = product.NameAr,
                        ProductNameEn = product.NameEn,
                        ProductImageUrl = product.ImageUrl,
                        UnitPrice = product.Price,
                        Quantity = cartItem.Quantity,
                        LineTotal = product.Price * cartItem.Quantity,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.OrderItems.Add(orderItem);

                    product.StockQuantity -= cartItem.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                }
            }

            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new CheckoutOrderResultDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Subtotal = order.Subtotal,
                DeliveryFee = order.DeliveryFee,
                TotalAmount = order.TotalAmount,
                Status = "Pending",
                CreatedAt = order.CreatedAt
            };
        }
    }
}