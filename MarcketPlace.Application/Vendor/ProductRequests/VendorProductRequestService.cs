using MarcketPlace.Application.Admin.Notifications;
using MarcketPlace.Application.Vendor.ProductRequests.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Vendor.ProductRequests
{
    public class VendorProductRequestService : IVendorProductRequestService
    {
        private readonly AppDbContext _context;
        private readonly IAdminNotificationService _notificationService;

        public VendorProductRequestService(AppDbContext context, IAdminNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<VendorProductRequestDto> CreateAsync(
            int vendorId,
            CreateVendorProductRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var vendor = await _context.Vendors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == vendorId, cancellationToken);

            if (vendor is null)
                throw new Exception("التاجر غير موجود.");

            if (!vendor.IsApproved)
                throw new Exception("لا يمكن إضافة منتج قبل اعتماد حساب التاجر.");

            var nameAr = dto.NameAr?.Trim();
            var nameEn = dto.NameEn?.Trim();

            if (string.IsNullOrWhiteSpace(nameAr))
                throw new Exception("الاسم العربي للمنتج مطلوب.");

            if (string.IsNullOrWhiteSpace(nameEn))
                throw new Exception("الاسم الإنجليزي للمنتج مطلوب.");

            if (dto.Price < 0)
                throw new Exception("السعر لا يمكن أن يكون أقل من صفر.");

            if (dto.StockQuantity < 0)
                throw new Exception("الكمية المتوفرة لا يمكن أن تكون أقل من صفر.");

            if (dto.MinStockQuantity < 0)
                throw new Exception("الحد الأدنى للكمية لا يمكن أن يكون أقل من صفر.");

            var store = await _context.Stores
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.StoreId && x.VendorId == vendorId, cancellationToken);

            if (store is null)
                throw new Exception("المتجر غير موجود أو لا يتبع لهذا التاجر.");

            if (!store.IsActive)
                throw new Exception("لا يمكن الإضافة إلى متجر غير نشط.");

            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == dto.CategoryId && x.IsActive, cancellationToken);

            if (category is null)
                throw new Exception("التصنيف غير موجود أو غير نشط.");

            var duplicatePendingRequestExists = await _context.ProductRequests
                .AnyAsync(x =>
                    x.Status == ProductApprovalStatus.Pending &&
                    x.StoreId == dto.StoreId &&
                    x.CategoryId == dto.CategoryId &&
                    (x.NameAr == nameAr || x.NameEn == nameEn),
                    cancellationToken);

            if (duplicatePendingRequestExists)
                throw new Exception("يوجد بالفعل طلب إضافة معلق لنفس المنتج في نفس المتجر والتصنيف.");

            var duplicateProductExists = await _context.Products
                .AnyAsync(x =>
                    x.StoreId == dto.StoreId &&
                    x.CategoryId == dto.CategoryId &&
                    (x.NameAr == nameAr || x.NameEn == nameEn),
                    cancellationToken);

            if (duplicateProductExists)
                throw new Exception("يوجد منتج بنفس الاسم العربي أو الإنجليزي داخل نفس المتجر والتصنيف.");

            var request = new ProductRequest
            {
                VendorId = vendorId,
                StoreId = dto.StoreId,
                CategoryId = dto.CategoryId,
                NameAr = nameAr,
                NameEn = nameEn,
                DescriptionAr = dto.DescriptionAr?.Trim(),
                DescriptionEn = dto.DescriptionEn?.Trim(),
                Image = dto.Image is { Length: > 0 } ? dto.Image : null,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                MinStockQuantity = dto.MinStockQuantity,
                Status = ProductApprovalStatus.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.ProductRequests.Add(request);
            await _context.SaveChangesAsync(cancellationToken);

            await _notificationService.CreateProductRequestNotificationAsync(
                request.Id,
                nameAr,
                store.NameAr,
                category.NameAr,
                cancellationToken);

            return MapToDto(request);
        }

        public async Task<VendorProductRequestDto> CreateByUserAsync(
            int userId,
            CreateVendorProductRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var vendorId = await GetVendorIdByUserIdAsync(userId, cancellationToken);
            return await CreateAsync(vendorId, dto, cancellationToken);
        }

        public async Task<IReadOnlyList<VendorProductRequestDto>> GetMyRequestsAsync(
            int vendorId,
            CancellationToken cancellationToken = default)
        {
            return await _context.ProductRequests
                .AsNoTracking()
                .Where(x => x.VendorId == vendorId)
                .OrderByDescending(x => x.RequestedAt)
                .Select(x => new VendorProductRequestDto
                {
                    Id = x.Id,
                    VendorId = x.VendorId,
                    StoreId = x.StoreId,
                    CategoryId = x.CategoryId,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    Price = x.Price,
                    StockQuantity = x.StockQuantity,
                    MinStockQuantity = x.MinStockQuantity,
                    Status = x.Status,
                    AdminNote = x.AdminNote,
                    RequestedAt = x.RequestedAt,
                    ReviewedAt = x.ReviewedAt,
                    ProductId = x.ProductId
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<VendorProductRequestDto>> GetMyRequestsByUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var vendorId = await GetVendorIdByUserIdAsync(userId, cancellationToken);
            return await GetMyRequestsAsync(vendorId, cancellationToken);
        }

        private async Task<int> GetVendorIdByUserIdAsync(
            int userId,
            CancellationToken cancellationToken)
        {
            var vendorId = await _context.Vendors
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (vendorId is null)
                throw new Exception("المستخدم الحالي لا يملك حساب تاجر.");

            return vendorId.Value;
        }

        private static VendorProductRequestDto MapToDto(ProductRequest request)
        {
            return new VendorProductRequestDto
            {
                Id = request.Id,
                VendorId = request.VendorId,
                StoreId = request.StoreId,
                CategoryId = request.CategoryId,
                NameAr = request.NameAr,
                NameEn = request.NameEn,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                MinStockQuantity = request.MinStockQuantity,
                Status = request.Status,
                AdminNote = request.AdminNote,
                RequestedAt = request.RequestedAt,
                ReviewedAt = request.ReviewedAt,
                ProductId = request.ProductId
            };
        }
    }
}