using MarcketPlace.Application.Customer.Orders.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Customer.Orders
{
    public class CustomerOrderService : ICustomerOrderService
    {
        private readonly AppDbContext _context;

        public CustomerOrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerCreatedOrderDto> CreateFromCartAsync(
            int customerUserId,
            CreateCustomerOrderFromCartDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            if (string.IsNullOrWhiteSpace(dto.AddressText))
                throw new InvalidOperationException("عنوان التوصيل مطلوب.");

            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == customerUserId, cancellationToken);

            if (customer is null)
                throw new KeyNotFoundException("الزبون غير موجود.");

            var deliveryZone = await _context.DeliveryZones
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.DeliveryZoneId, cancellationToken);

            if (deliveryZone is null)
                throw new KeyNotFoundException("منطقة التوصيل غير موجودة.");

            var cartItems = await _context.CartItems
                .Include(x => x.Product)
                    .ThenInclude(x => x!.Store)
                .Include(x => x.Product)
                    .ThenInclude(x => x!.Category)
                .Where(x => x.CustomerId == customer.Id)
                .ToListAsync(cancellationToken);

            if (cartItems.Count == 0)
                throw new InvalidOperationException("السلة فارغة.");

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Quantity <= 0)
                    throw new InvalidOperationException("يوجد عنصر في السلة بكمية غير صحيحة.");

                if (cartItem.Product is null)
                    throw new InvalidOperationException("يوجد منتج غير صالح في السلة.");

                if (cartItem.Product.StoreId is null)
                    throw new InvalidOperationException("يوجد منتج غير مرتبط بمتجر داخل السلة.");

                if (cartItem.Product.Store is null || !cartItem.Product.Store.IsActive)
                    throw new InvalidOperationException($"المتجر الخاص بالمنتج {cartItem.Product.NameAr} غير متاح.");

                if (cartItem.Product.ApprovalStatus != ProductApprovalStatus.Approved)
                    throw new InvalidOperationException($"المنتج {cartItem.Product.NameAr} غير معتمد.");

                if (cartItem.Product.StockQuantity < cartItem.Quantity)
                    throw new InvalidOperationException($"الكمية المطلوبة من المنتج {cartItem.Product.NameAr} غير متوفرة.");
            }

            var utcNow = DateTime.UtcNow;
            var subtotal = cartItems.Sum(x => x.Product.Price * x.Quantity);
            var deliveryFee = deliveryZone.DeliveryFee;
            var totalAmount = subtotal + deliveryFee;

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                CustomerId = customer.Id,
                DriverId = null,
                DeliveryZoneId = dto.DeliveryZoneId,
                Status = OrderStatus.Pending,
                AddressText = dto.AddressText.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CustomerNote = string.IsNullOrWhiteSpace(dto.CustomerNote) ? null : dto.CustomerNote.Trim(),
                Subtotal = subtotal,
                DeliveryFee = deliveryFee,
                TotalAmount = totalAmount,
                DriverAssignedAt = null,
                PickedUpAt = null,
                DeliveredAt = null,
                CancelReason = null,
                CancelledByUserId = null,
                CancelledAt = null,
                CreatedAt = utcNow,
                UpdatedAt = null
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            var groupedByStore = cartItems
                .GroupBy(x => x.Product.StoreId!.Value)
                .ToList();

            var orderStores = new List<OrderStore>();

            foreach (var storeGroup in groupedByStore)
            {
                var firstItem = storeGroup.First();
                var storeSubtotal = storeGroup.Sum(x => x.Product.Price * x.Quantity);

                var orderStore = new OrderStore
                {
                    OrderId = order.Id,
                    StoreId = storeGroup.Key,
                    Status = OrderStoreStatus.Pending,
                    Note = null,
                    ReadyAt = null,
                    StoreSubtotal = storeSubtotal,
                    CreatedAt = utcNow,
                    UpdatedAt = null
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
                        CreatedAt = utcNow
                    };

                    _context.OrderItems.Add(orderItem);

                    product.StockQuantity -= cartItem.Quantity;
                    product.UpdatedAt = utcNow;
                }

                orderStores.Add(orderStore);
            }

            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var result = new CustomerCreatedOrderDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                Subtotal = order.Subtotal,
                DeliveryFee = order.DeliveryFee,
                TotalAmount = order.TotalAmount,
                AddressText = order.AddressText,
                CustomerNote = order.CustomerNote,
                CreatedAt = order.CreatedAt,
                Stores = orderStores.Select(x => new CustomerCreatedOrderStoreDto
                {
                    OrderStoreId = x.Id,
                    StoreId = x.StoreId,
                    StoreNameAr = groupedByStore
                        .First(g => g.Key == x.StoreId)
                        .First()
                        .Product.Store!.NameAr,
                    StoreNameEn = groupedByStore
                        .First(g => g.Key == x.StoreId)
                        .First()
                        .Product.Store!.NameEn,
                    StoreSubtotal = x.StoreSubtotal,
                    Status = x.Status
                }).ToList()
            };

            return result;
        }

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }
    }
}