using MarcketPlace.Application.Customer.Orders.Dtos;
using MarcketPlace.Application.Shared.Orders.Dtos;
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

            if (dto.DeliveryZoneId <= 0)
                throw new InvalidOperationException("منطقة التوصيل غير صالحة.");

            if (string.IsNullOrWhiteSpace(dto.AddressText))
                throw new InvalidOperationException("عنوان التوصيل مطلوب.");

            if (dto.Latitude < -90 || dto.Latitude > 90)
                throw new InvalidOperationException("خط العرض غير صالح.");

            if (dto.Longitude < -180 || dto.Longitude > 180)
                throw new InvalidOperationException("خط الطول غير صالح.");

            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var deliveryZone = await _context.DeliveryZones
                .FirstOrDefaultAsync(x => x.Id == dto.DeliveryZoneId, cancellationToken);

            if (deliveryZone is null)
                throw new KeyNotFoundException("منطقة التوصيل غير موجودة.");

            var setting = await _context.SystemSettings
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (setting is null)
                throw new InvalidOperationException("إعدادات النظام غير موجودة.");

            var cartItems = await _context.CartItems
                .Include(x => x.Product)
                    .ThenInclude(x => x.Unit)
                .Include(x => x.ProductVariant)
                    .ThenInclude(x => x.Unit)
                .Where(x => x.CustomerId == customerId)
                .OrderBy(x => x.Id)
                .ToListAsync(cancellationToken);

            if (cartItems.Count == 0)
                throw new InvalidOperationException("السلة فارغة.");

            var simpleProductIds = cartItems
                .Where(x => !x.ProductVariantId.HasValue)
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            var variantIds = cartItems
                .Where(x => x.ProductVariantId.HasValue)
                .Select(x => x.ProductVariantId!.Value)
                .Distinct()
                .ToList();

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var simpleProducts = simpleProductIds.Count == 0
                ? new Dictionary<int, Product>()
                : await _context.Products
                    .Where(x => simpleProductIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, cancellationToken);

            var variants = variantIds.Count == 0
                ? new Dictionary<int, ProductVariant>()
                : await _context.ProductVariants
                    .Include(x => x.Product)
                    .Where(x => variantIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, cancellationToken);

            var now = DateTime.UtcNow;

            foreach (var cartItem in cartItems)
            {
                if (cartItem.ProductVariantId.HasValue)
                {
                    if (!variants.TryGetValue(cartItem.ProductVariantId.Value, out var variant))
                        throw new KeyNotFoundException("أحد الـ Variants لم يعد موجودًا.");

                    if (!variant.IsActive || variant.Product is null || !variant.Product.IsActive)
                        throw new InvalidOperationException("أحد الـ Variants لم يعد مفعّلًا.");

                    if (variant.StockQuantity < cartItem.Quantity)
                        throw new InvalidOperationException("إحدى كميات الطلب لم تعد متوفرة.");

                    variant.StockQuantity -= cartItem.Quantity;
                    variant.UpdatedAt = now;
                }
                else
                {
                    if (!simpleProducts.TryGetValue(cartItem.ProductId, out var product))
                        throw new KeyNotFoundException("أحد المنتجات لم يعد موجودًا.");

                    if (!product.IsActive)
                        throw new InvalidOperationException("أحد المنتجات لم يعد مفعّلًا.");

                    if (product.StockQuantity < cartItem.Quantity)
                        throw new InvalidOperationException("إحدى كميات الطلب لم تعد متوفرة.");

                    product.StockQuantity -= cartItem.Quantity;
                    product.UpdatedAt = now;
                }
            }

            var affectedVariableProductIds = variants.Values
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            if (affectedVariableProductIds.Count > 0)
            {
                var affectedProducts = await _context.Products
                    .Include(x => x.Variants)
                    .Where(x => affectedVariableProductIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

                foreach (var product in affectedProducts)
                {
                    product.StockQuantity = product.Variants
                        .Where(v => v.IsActive)
                        .Sum(v => v.StockQuantity);

                    product.MinStockQuantity = product.Variants
                        .Where(v => v.IsActive)
                        .Sum(v => v.MinStockQuantity);

                    product.UpdatedAt = now;
                }
            }

            var subtotal = RoundMoney(cartItems.Sum(x => x.LineTotal));
            var deliveryFee = RoundMoney(deliveryZone.DeliveryFee);
            var totalAmount = RoundMoney(subtotal + deliveryFee);

            var order = new Order
            {
                OrderNumber = await GenerateOrderNumberAsync(cancellationToken),
                CustomerId = customerId,
                DriverId = null,
                DeliveryZoneId = deliveryZone.Id,
                Status = OrderStatus.Pending,

                AddressText = dto.AddressText.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,

                PickupLocationNameAr = setting.PickupNameAr,
                PickupLocationNameEn = setting.PickupNameEn,
                PickupAddressText = setting.PickupAddressText,
                PickupLatitude = setting.PickupLatitude,
                PickupLongitude = setting.PickupLongitude,

                CustomerNote = string.IsNullOrWhiteSpace(dto.CustomerNote)
                    ? null
                    : dto.CustomerNote.Trim(),

                CustomerWhatsAppNumber = string.IsNullOrWhiteSpace(dto.CustomerWhatsAppNumber)
                    ? null
                    : dto.CustomerWhatsAppNumber.Trim(),

                Subtotal = subtotal,
                DeliveryFee = deliveryFee,
                TotalAmount = totalAmount,

                CreatedAt = now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var cartItem in cartItems)
            {
                var product = cartItem.Product!;
                var variant = cartItem.ProductVariant;

                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    ProductVariantId = cartItem.ProductVariantId,

                    ProductNameAr = product.NameAr,
                    ProductNameEn = product.NameEn,
                    ProductImage = product.Image,

                    VariantNameAr = variant?.NameAr,
                    VariantNameEn = variant?.NameEn,

                    UnitSymbol = variant?.Unit?.Symbol ?? product.Unit?.Symbol,

                    UnitPrice = cartItem.UnitPrice,
                    Quantity = cartItem.Quantity,
                    RequestedAmount = cartItem.RequestedAmount,
                    PurchaseInputMode = cartItem.PurchaseEntryMode,
                    LineTotal = cartItem.LineTotal,
                    CreatedAt = now
                });
            }

            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new CustomerCreatedOrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                Subtotal = order.Subtotal,
                DeliveryFee = order.DeliveryFee,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt
            };
        }

        public async Task<IReadOnlyList<OrderListItemDto>> GetMyOrdersAsync(
            int customerUserId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.User)
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .Include(x => x.DeliveryZone)
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return orders.Select(MapListItem).ToList();
        }

        public async Task<OrderDetailsDto> GetByIdAsync(
            int customerUserId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var order = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Customer)
                    .ThenInclude(x => x.User)
                .Include(x => x.Driver)
                    .ThenInclude(x => x.User)
                .Include(x => x.DeliveryZone)
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == orderId && x.CustomerId == customerId, cancellationToken);

            if (order is null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            return MapDetails(order);
        }

        public async Task<OrderDetailsDto> CancelAsync(
            int customerUserId,
            int orderId,
            CancelCustomerOrderDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var order = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == orderId && x.CustomerId == customerId, cancellationToken);

            if (order is null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            if (order.CancelledAt.HasValue || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("الطلب ملغي مسبقًا.");

            if (order.Status == OrderStatus.PickedUp || order.Status == OrderStatus.Delivered)
                throw new InvalidOperationException("لا يمكن للزبون إلغاء الطلب بعد أن يصبح PickedUp.");

            var reason = string.IsNullOrWhiteSpace(dto.Reason)
                ? "تم إلغاء الطلب من قبل الزبون."
                : dto.Reason.Trim();

            var now = DateTime.UtcNow;

            var affectedRows = await _context.Orders
                .Where(x =>
                    x.Id == orderId &&
                    x.CustomerId == customerId &&
                    x.CancelledAt == null &&
                    (x.Status == OrderStatus.Pending || x.Status == OrderStatus.DriverAssigned))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, OrderStatus.Cancelled)
                    .SetProperty(x => x.CancelReason, reason)
                    .SetProperty(x => x.CancelledByUserId, customerUserId)
                    .SetProperty(x => x.CancelledAt, now)
                    .SetProperty(x => x.UpdatedAt, now),
                    cancellationToken);

            if (affectedRows == 0)
                throw new InvalidOperationException("لا يمكن إلغاء الطلب بعد أن يصبح PickedUp.");

            await RestoreOrderStockAsync(order.Items, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return await GetByIdAsync(customerUserId, orderId, cancellationToken);
        }

        private async Task<int> GetCustomerIdAsync(
            int customerUserId,
            CancellationToken cancellationToken)
        {
            var customerId = await _context.Customers
                .Where(x => x.UserId == customerUserId)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (!customerId.HasValue)
                throw new KeyNotFoundException("الزبون غير موجود.");

            return customerId.Value;
        }

        private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
        {
            for (var i = 0; i < 10; i++)
            {
                var candidate = $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}";

                var exists = await _context.Orders
                    .AsNoTracking()
                    .AnyAsync(x => x.OrderNumber == candidate, cancellationToken);

                if (!exists)
                    return candidate;
            }

            return $"ORD-{Guid.NewGuid():N}".ToUpperInvariant()[..18];
        }

        private async Task RestoreOrderStockAsync(
            IEnumerable<OrderItem> orderItems,
            CancellationToken cancellationToken)
        {
            var items = orderItems.ToList();
            if (items.Count == 0)
                return;

            var simpleProductIds = items
                .Where(x => !x.ProductVariantId.HasValue)
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            var variantIds = items
                .Where(x => x.ProductVariantId.HasValue)
                .Select(x => x.ProductVariantId!.Value)
                .Distinct()
                .ToList();

            var simpleProducts = simpleProductIds.Count == 0
                ? new Dictionary<int, Product>()
                : await _context.Products
                    .Where(x => simpleProductIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, cancellationToken);

            var variants = variantIds.Count == 0
                ? new Dictionary<int, ProductVariant>()
                : await _context.ProductVariants
                    .Where(x => variantIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, cancellationToken);

            var now = DateTime.UtcNow;

            foreach (var item in items)
            {
                if (item.ProductVariantId.HasValue)
                {
                    if (variants.TryGetValue(item.ProductVariantId.Value, out var variant))
                    {
                        variant.StockQuantity += item.Quantity;
                        variant.UpdatedAt = now;
                    }
                }
                else
                {
                    if (simpleProducts.TryGetValue(item.ProductId, out var product))
                    {
                        product.StockQuantity += item.Quantity;
                        product.UpdatedAt = now;
                    }
                }
            }

            var affectedVariableProductIds = variants.Values
                .Select(x => x.ProductId)
                .Distinct()
                .ToList();

            if (affectedVariableProductIds.Count > 0)
            {
                var affectedProducts = await _context.Products
                    .Include(x => x.Variants)
                    .Where(x => affectedVariableProductIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

                foreach (var product in affectedProducts)
                {
                    product.StockQuantity = product.Variants
                        .Where(v => v.IsActive)
                        .Sum(v => v.StockQuantity);

                    product.MinStockQuantity = product.Variants
                        .Where(v => v.IsActive)
                        .Sum(v => v.MinStockQuantity);

                    product.UpdatedAt = now;
                }
            }
        }

        private decimal RoundMoney(decimal value)
        {
            return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static OrderListItemDto MapListItem(Order order)
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

        private static OrderDetailsDto MapDetails(Order order)
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
                DriverVehicleType = order.Driver?.VehicleType,
                DriverVehicleNumber = order.Driver?.VehicleNumber,

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