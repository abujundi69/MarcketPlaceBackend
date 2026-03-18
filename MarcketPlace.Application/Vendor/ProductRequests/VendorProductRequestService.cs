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
        private const string SystemStoreNameAr = "مستودع نبض المدينة";

        private readonly AppDbContext _context;
        private readonly IAdminNotificationService _notificationService;

        public VendorProductRequestService(
            AppDbContext context,
            IAdminNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<VendorProductRequestDto> CreateAsync(
            int vendorUserId,
            CreateVendorProductRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(x => x.UserId == vendorUserId, cancellationToken)
                ?? throw new InvalidOperationException("التاجر غير موجود.");

            var store = await _context.Stores
                .FirstOrDefaultAsync(x => x.VendorId == null && x.NameAr == SystemStoreNameAr, cancellationToken)
                ?? throw new InvalidOperationException("المستودع الافتراضي غير موجود. يرجى تشغيل تهيئة قاعدة البيانات.");

            await ValidateCategoryAsync(dto.CategoryId, cancellationToken);
            if (dto.UnitId.HasValue)
                await ValidateUnitAsync(dto.UnitId.Value, cancellationToken);

            ValidatePayload(dto);

            var imageBytes = ParseBase64Image(dto.ImageBase64);
            var now = DateTime.UtcNow;

            var productRequest = new ProductRequest
            {
                VendorId = vendor.Id,
                StoreId = store.Id,
                CategoryId = dto.CategoryId,
                UnitId = dto.UnitId,

                NameAr = dto.NameAr.Trim(),
                NameEn = dto.NameEn.Trim(),
                DescriptionAr = dto.DescriptionAr?.Trim(),
                DescriptionEn = dto.DescriptionEn?.Trim(),
                Image = imageBytes,

                ProductType = ProductType.Simple,
                PurchaseInputMode = dto.PurchaseInputMode,
                AllowDecimalQuantity = dto.AllowDecimalQuantity,

                Price = dto.Price,
                SalePrice = dto.SalePrice,
                CostPrice = dto.CostPrice,

                StockQuantity = dto.StockQuantity,
                MinStockQuantity = dto.MinStockQuantity,
                MinPurchaseQuantity = dto.MinPurchaseQuantity,
                MaxPurchaseQuantity = dto.MaxPurchaseQuantity,
                QuantityStep = dto.QuantityStep,

                Status = ProductApprovalStatus.Pending,
                RequestedAt = now
            };

            _context.ProductRequests.Add(productRequest);
            await _context.SaveChangesAsync(cancellationToken);

            var category = await _context.Categories
                .AsNoTracking()
                .Where(x => x.Id == dto.CategoryId)
                .Select(x => x.NameAr)
                .FirstAsync(cancellationToken);

            await _notificationService.CreateProductRequestNotificationAsync(
                productRequest.Id,
                productRequest.NameAr,
                store.NameAr,
                category,
                cancellationToken);

            return await MapToDtoAsync(productRequest.Id, cancellationToken);
        }

        public async Task<IReadOnlyList<VendorProductRequestDto>> GetMineAsync(
            int vendorUserId,
            CancellationToken cancellationToken = default)
        {
            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(x => x.UserId == vendorUserId, cancellationToken)
                ?? throw new InvalidOperationException("التاجر غير موجود.");

            return await _context.ProductRequests
                .AsNoTracking()
                .Where(x => x.VendorId == vendor.Id)
                .OrderByDescending(x => x.RequestedAt)
                .Select(x => new VendorProductRequestDto
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    UnitId = x.UnitId,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    DescriptionAr = x.DescriptionAr,
                    DescriptionEn = x.DescriptionEn,
                    Image = x.Image,
                    Status = x.Status,
                    AdminNote = x.AdminNote,
                    CategoryNameAr = x.Category.NameAr,
                    CategoryNameEn = x.Category.NameEn,
                    UnitNameAr = x.Unit != null ? x.Unit.NameAr : null,
                    UnitNameEn = x.Unit != null ? x.Unit.NameEn : null,
                    RequestedAt = x.RequestedAt,
                    ReviewedAt = x.ReviewedAt,
                    ProductId = x.ProductId
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<VendorProductRequestDto?> GetByIdAsync(
            int vendorUserId,
            int productRequestId,
            CancellationToken cancellationToken = default)
        {
            var vendor = await _context.Vendors
                .FirstOrDefaultAsync(x => x.UserId == vendorUserId, cancellationToken);

            if (vendor is null)
                return null;

            return await MapToDtoAsync(productRequestId, cancellationToken, vendor.Id);
        }

        private async Task<VendorProductRequestDto> MapToDtoAsync(int id, CancellationToken ct, int? vendorId = null)
        {
            var query = _context.ProductRequests.AsNoTracking().Where(x => x.Id == id);
            if (vendorId.HasValue)
                query = query.Where(x => x.VendorId == vendorId.Value);

            var dto = await query
                .Select(x => new VendorProductRequestDto
                {
                    Id = x.Id,
                    CategoryId = x.CategoryId,
                    UnitId = x.UnitId,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    DescriptionAr = x.DescriptionAr,
                    DescriptionEn = x.DescriptionEn,
                    Image = x.Image,
                    Status = x.Status,
                    AdminNote = x.AdminNote,
                    CategoryNameAr = x.Category.NameAr,
                    CategoryNameEn = x.Category.NameEn,
                    UnitNameAr = x.Unit != null ? x.Unit.NameAr : null,
                    UnitNameEn = x.Unit != null ? x.Unit.NameEn : null,
                    RequestedAt = x.RequestedAt,
                    ReviewedAt = x.ReviewedAt,
                    ProductId = x.ProductId
                })
                .FirstOrDefaultAsync(ct);

            if (dto is null)
                throw new KeyNotFoundException("طلب المنتج غير موجود.");

            return dto;
        }

        private async Task ValidateCategoryAsync(int categoryId, CancellationToken ct)
        {
            var exists = await _context.Categories.AnyAsync(x => x.Id == categoryId && x.IsActive, ct);
            if (!exists)
                throw new InvalidOperationException("التصنيف غير موجود أو غير مفعل.");
        }

        private async Task ValidateUnitAsync(int unitId, CancellationToken ct)
        {
            var exists = await _context.ProductUnits.AnyAsync(x => x.Id == unitId && x.IsActive, ct);
            if (!exists)
                throw new InvalidOperationException("وحدة المنتج غير موجودة أو غير مفعلة.");
        }

        private void ValidatePayload(CreateVendorProductRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NameAr))
                throw new InvalidOperationException("الاسم العربي مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.NameEn))
                throw new InvalidOperationException("الاسم الإنجليزي مطلوب.");
            if (dto.CategoryId <= 0)
                throw new InvalidOperationException("الكاتيجوري غير صالحة.");
            if (dto.Price <= 0)
                throw new InvalidOperationException("السعر يجب أن يكون أكبر من صفر.");
            if (dto.QuantityStep <= 0)
                throw new InvalidOperationException("QuantityStep يجب أن يكون أكبر من صفر.");
            if (dto.MinPurchaseQuantity <= 0)
                throw new InvalidOperationException("MinPurchaseQuantity يجب أن يكون أكبر من صفر.");
        }

        private static byte[]? ParseBase64Image(string? base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return null;
            try
            {
                var cleaned = base64.Trim();
                var commaIndex = cleaned.IndexOf(',');
                if (commaIndex >= 0)
                    cleaned = cleaned[(commaIndex + 1)..];
                return Convert.FromBase64String(cleaned);
            }
            catch
            {
                throw new InvalidOperationException("الصورة المرسلة ليست Base64 صحيحة.");
            }
        }
    }
}
