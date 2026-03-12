using MarcketPlace.Application.Vendor.Orders.Dtos;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Vendor.Orders
{
    public class VendorStoreOrderService : IVendorStoreOrderService
    {
        private readonly AppDbContext _context;

        public VendorStoreOrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<VendorStoreOrderListItemDto>> GetStoreOrdersAsync(
            int vendorUserId,
            int storeId,
            CancellationToken cancellationToken = default)
        {
            var storeBelongsToVendor = await _context.Stores
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == storeId && x.Vendor.UserId == vendorUserId,
                    cancellationToken);

            if (!storeBelongsToVendor)
                throw new KeyNotFoundException("المتجر غير موجود أو لا يتبع لهذا التاجر.");

            var orders = await _context.OrderStores
                .AsNoTracking()
                .Where(x => x.StoreId == storeId && x.Store.Vendor.UserId == vendorUserId)
                .OrderByDescending(x => x.Order.CreatedAt)
                .Select(x => new VendorStoreOrderListItemDto
                {
                    OrderStoreId = x.Id,
                    OrderId = x.OrderId,
                    OrderNumber = x.Order.OrderNumber,

                    StoreId = x.StoreId,
                    StoreNameAr = x.Store.NameAr,
                    StoreNameEn = x.Store.NameEn,

                    OrderStatus = x.Order.Status,
                    StoreOrderStatus = x.Status,

                    CustomerName = x.Order.Customer.User.FullName,
                    CustomerPhoneNumber = x.Order.Customer.User.PhoneNumber,

                    AddressText = x.Order.AddressText,
                    CustomerNote = x.Order.CustomerNote,

                    StoreSubtotal = x.StoreSubtotal,
                    TotalItemsCount = x.OrderItems.Sum(i => i.Quantity),

                    CreatedAt = x.Order.CreatedAt,
                    DriverAssignedAt = x.Order.DriverAssignedAt,
                    PickedUpAt = x.Order.PickedUpAt,
                    DeliveredAt = x.Order.DeliveredAt,
                    CancelledAt = x.Order.CancelledAt,
                    CancelReason = x.Order.CancelReason
                })
                .ToListAsync(cancellationToken);

            return orders;
        }

        public async Task<VendorStoreOrderDetailsDto> GetStoreOrderDetailsAsync(
            int vendorUserId,
            int storeId,
            int orderStoreId,
            CancellationToken cancellationToken = default)
        {
            var orderStore = await _context.OrderStores
                .AsNoTracking()
                .Where(x =>
                    x.Id == orderStoreId &&
                    x.StoreId == storeId &&
                    x.Store.Vendor.UserId == vendorUserId)
                .Select(x => new VendorStoreOrderDetailsDto
                {
                    OrderStoreId = x.Id,
                    OrderId = x.OrderId,
                    OrderNumber = x.Order.OrderNumber,

                    StoreId = x.StoreId,
                    StoreNameAr = x.Store.NameAr,
                    StoreNameEn = x.Store.NameEn,

                    OrderStatus = x.Order.Status,
                    StoreOrderStatus = x.Status,

                    CustomerId = x.Order.CustomerId,
                    CustomerName = x.Order.Customer.User.FullName,
                    CustomerPhoneNumber = x.Order.Customer.User.PhoneNumber,

                    AddressText = x.Order.AddressText,
                    Latitude = x.Order.Latitude,
                    Longitude = x.Order.Longitude,
                    CustomerNote = x.Order.CustomerNote,

                    StoreSubtotal = x.StoreSubtotal,

                    CreatedAt = x.Order.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    DriverAssignedAt = x.Order.DriverAssignedAt,
                    PickedUpAt = x.Order.PickedUpAt,
                    DeliveredAt = x.Order.DeliveredAt,
                    CancelledAt = x.Order.CancelledAt,
                    CancelReason = x.Order.CancelReason,

                    Items = x.OrderItems
                        .OrderBy(i => i.Id)
                        .Select(i => new VendorStoreOrderItemDto
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
                .FirstOrDefaultAsync(cancellationToken);

            if (orderStore is null)
                throw new KeyNotFoundException("الطلب الخاص بهذا المتجر غير موجود.");

            return orderStore;
        }

        public async Task<VendorStoreOrderDetailsDto> UpdateStoreOrderStatusAsync(
            int vendorUserId,
            int storeId,
            int orderStoreId,
            UpdateVendorStoreOrderStatusDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            if (!Enum.IsDefined(typeof(OrderStoreStatus), dto.Status))
                throw new InvalidOperationException("حالة الطلب الخاصة بالمتجر غير صحيحة.");

            var orderStore = await _context.OrderStores
                .Include(x => x.Order)
                .FirstOrDefaultAsync(x =>
                    x.Id == orderStoreId &&
                    x.StoreId == storeId &&
                    x.Store.Vendor.UserId == vendorUserId,
                    cancellationToken);

            if (orderStore is null)
                throw new KeyNotFoundException("الطلب الخاص بهذا المتجر غير موجود أو لا يتبع لهذا التاجر.");

            if (orderStore.Order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("لا يمكن تعديل طلب تم إلغاؤه.");

            ValidateTransition(orderStore.Status, dto.Status, dto.Note);

            orderStore.Status = dto.Status;
            orderStore.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
            orderStore.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == OrderStoreStatus.Ready && orderStore.ReadyAt is null)
                orderStore.ReadyAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return await GetStoreOrderDetailsAsync(
                vendorUserId,
                storeId,
                orderStoreId,
                cancellationToken);
        }

        private static void ValidateTransition(
            OrderStoreStatus currentStatus,
            OrderStoreStatus newStatus,
            string? note)
        {
            if (currentStatus == newStatus)
                throw new InvalidOperationException("الطلب موجود بالفعل على نفس الحالة.");

            if (newStatus == OrderStoreStatus.Pending)
                throw new InvalidOperationException("لا يمكن إرجاع الطلب إلى Pending.");

            if ((newStatus == OrderStoreStatus.Cancelled || newStatus == OrderStoreStatus.IssueReported)
                && string.IsNullOrWhiteSpace(note))
            {
                throw new InvalidOperationException("يرجى إدخال ملاحظة عند الإلغاء أو الإبلاغ عن مشكلة.");
            }

            var isAllowed = currentStatus switch
            {
                OrderStoreStatus.Pending =>
                    newStatus == OrderStoreStatus.Preparing ||
                    newStatus == OrderStoreStatus.Cancelled ||
                    newStatus == OrderStoreStatus.IssueReported,

                OrderStoreStatus.Preparing =>
                    newStatus == OrderStoreStatus.Ready ||
                    newStatus == OrderStoreStatus.Cancelled ||
                    newStatus == OrderStoreStatus.IssueReported,

                OrderStoreStatus.IssueReported =>
                    newStatus == OrderStoreStatus.Preparing ||
                    newStatus == OrderStoreStatus.Cancelled,

                OrderStoreStatus.Ready => false,
                OrderStoreStatus.Cancelled => false,
                _ => false
            };

            if (!isAllowed)
                throw new InvalidOperationException(
                    $"لا يمكن تغيير حالة الطلب من {currentStatus} إلى {newStatus}.");
        }
    }
}