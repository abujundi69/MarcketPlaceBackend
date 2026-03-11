using MarcketPlace.Application.Customer.Orders.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DriverEntity = MarcketPlace.Domain.Entities.Driver;

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

            var customer = await _context.Customers
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
                .Where(x => x.CustomerId == customer.Id)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            if (cartItems.Count == 0)
                throw new InvalidOperationException("السلة فارغة.");

            var validatedItems = new List<(CartItem CartItem, Product Product, Store Store)>();

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Quantity <= 0)
                    throw new InvalidOperationException("يوجد عنصر في السلة بكمية غير صحيحة.");

                var product = cartItem.Product
                    ?? throw new InvalidOperationException("يوجد منتج غير صالح في السلة.");

                if (product.StoreId is null || product.Store is null)
                    throw new InvalidOperationException($"المنتج {product.NameAr} غير مرتبط بمتجر.");

                if (!product.Store.IsActive)
                    throw new InvalidOperationException($"المتجر الخاص بالمنتج {product.NameAr} غير متاح.");

                if (product.ApprovalStatus != ProductApprovalStatus.Approved)
                    throw new InvalidOperationException($"المنتج {product.NameAr} غير معتمد.");

                if (product.StockQuantity < cartItem.Quantity)
                    throw new InvalidOperationException($"الكمية المطلوبة من المنتج {product.NameAr} غير متوفرة.");

                validatedItems.Add((cartItem, product, product.Store));
            }

            var utcNow = DateTime.UtcNow;
            var subtotal = validatedItems.Sum(x => x.Product.Price * x.CartItem.Quantity);
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
                UpdatedAt = null,
                OrderStores = new List<OrderStore>()
            };

            var groupedByStore = validatedItems
                .GroupBy(x => new
                {
                    x.Store.Id,
                    x.Store.NameAr,
                    x.Store.NameEn
                })
                .ToList();

            foreach (var storeGroup in groupedByStore)
            {
                var orderStore = new OrderStore
                {
                    StoreId = storeGroup.Key.Id,
                    Status = OrderStoreStatus.Pending,
                    Note = null,
                    ReadyAt = null,
                    StoreSubtotal = storeGroup.Sum(x => x.Product.Price * x.CartItem.Quantity),
                    CreatedAt = utcNow,
                    UpdatedAt = null,
                    OrderItems = new List<OrderItem>()
                };

                foreach (var entry in storeGroup)
                {
                    var cartItem = entry.CartItem;
                    var product = entry.Product;

                    orderStore.OrderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        ProductNameAr = product.NameAr,
                        ProductNameEn = product.NameEn,
                        ProductImage = product.Image,
                        UnitPrice = product.Price,
                        Quantity = cartItem.Quantity,
                        LineTotal = product.Price * cartItem.Quantity,
                        CreatedAt = utcNow
                    });

                    product.StockQuantity -= cartItem.Quantity;
                    product.UpdatedAt = utcNow;
                }

                order.OrderStores.Add(orderStore);
            }

            customer.DefaultDeliveryZoneId = dto.DeliveryZoneId;
            customer.DefaultAddressText = dto.AddressText.Trim();
            customer.DefaultLatitude = dto.Latitude;
            customer.DefaultLongitude = dto.Longitude;
            customer.LocationUpdatedAt = utcNow;

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cartItems);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new CustomerCreatedOrderDto
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
                Stores = order.OrderStores
                    .Select(x => new CustomerCreatedOrderStoreDto
                    {
                        OrderStoreId = x.Id,
                        StoreId = x.StoreId,
                        StoreNameAr = groupedByStore.First(g => g.Key.Id == x.StoreId).Key.NameAr,
                        StoreNameEn = groupedByStore.First(g => g.Key.Id == x.StoreId).Key.NameEn,
                        StoreSubtotal = x.StoreSubtotal,
                        Status = x.Status
                    })
                    .ToList()
            };
        }

        public async Task<IReadOnlyList<CustomerOrderListItemDto>> GetMyOrdersAsync(
            int customerUserId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var orders = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Driver)
                    .ThenInclude(x => x!.User)
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return orders.Select(MapCustomerOrderListItem).ToList();
        }

        public async Task<CustomerOrderDetailsDto> GetByIdAsync(
            int customerUserId,
            int orderId,
            CancellationToken cancellationToken = default)
        {
            if (orderId <= 0)
                throw new InvalidOperationException("رقم الطلب غير صالح.");

            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var order = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Driver)
                    .ThenInclude(x => x!.User)
                .Include(x => x.OrderStores)
                    .ThenInclude(x => x.Store)
                .Include(x => x.OrderStores)
                    .ThenInclude(x => x.OrderItems)
                .FirstOrDefaultAsync(
                    x => x.Id == orderId && x.CustomerId == customerId,
                    cancellationToken);

            if (order is null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            return MapCustomerOrderDetails(order);
        }

        private async Task<int> GetCustomerIdAsync(
            int customerUserId,
            CancellationToken cancellationToken)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == customerUserId, cancellationToken);

            if (customer is null)
                throw new KeyNotFoundException("الزبون غير موجود.");

            return customer.Id;
        }

        private static CustomerOrderListItemDto MapCustomerOrderListItem(Order order)
        {
            return new CustomerOrderListItemDto
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Status = order.Status,
                Subtotal = order.Subtotal,
                DeliveryFee = order.DeliveryFee,
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                DriverAssignedAt = order.DriverAssignedAt,
                PickedUpAt = order.PickedUpAt,
                DeliveredAt = order.DeliveredAt,
                Driver = MapCustomerDriver(order.Driver)
            };
        }

        private static CustomerOrderDetailsDto MapCustomerOrderDetails(Order order)
        {
            return new CustomerOrderDetailsDto
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
                Driver = MapCustomerDriver(order.Driver),
                Stores = order.OrderStores
                    .OrderBy(x => x.Id)
                    .Select(x => new CustomerOrderStoreDto
                    {
                        OrderStoreId = x.Id,
                        StoreId = x.StoreId,
                        StoreNameAr = x.Store?.NameAr ?? string.Empty,
                        StoreNameEn = x.Store?.NameEn ?? string.Empty,
                        Status = x.Status,
                        ReadyAt = x.ReadyAt,
                        StoreSubtotal = x.StoreSubtotal,
                        Items = x.OrderItems
                            .OrderBy(i => i.Id)
                            .Select(i => new CustomerOrderItemDto
                            {
                                OrderItemId = i.Id,
                                ProductId = i.ProductId,
                                ProductNameAr = i.ProductNameAr,
                                ProductNameEn = i.ProductNameEn,
                                ProductImage = i.ProductImage,
                                UnitPrice = i.UnitPrice,
                                Quantity = i.Quantity,
                                LineTotal = i.LineTotal
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }

        private static CustomerOrderDriverDto? MapCustomerDriver(DriverEntity? driver)
        {
            if (driver is null)
                return null;

            return new CustomerOrderDriverDto
            {
                DriverId = driver.Id,
                FullName = driver.User?.FullName ?? string.Empty,
                PhoneNumber = driver.User?.PhoneNumber ?? string.Empty,
                VehicleType = driver.VehicleType,
                VehicleNumber = driver.VehicleNumber
            };
        }

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }
    }
}