using MarcketPlace.Application.Admin.ProductRequests.Dtos;
using MarcketPlace.Application.Admin.Products;
using MarcketPlace.Application.Admin.Products.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.ProductRequests
{
    public class AdminProductRequestService : IAdminProductRequestService
    {
        private readonly AppDbContext _context;
        private readonly IAdminProductService _productService;

        public AdminProductRequestService(
            AppDbContext context,
            IAdminProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        public async Task<IReadOnlyList<AdminProductRequestDto>> GetPendingAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ProductRequests
                .AsNoTracking()
                .Where(x => x.Status == ProductApprovalStatus.Pending)
                .OrderByDescending(x => x.RequestedAt)
                .Select(x => new AdminProductRequestDto
                {
                    Id = x.Id,
                    VendorId = x.VendorId,
                    StoreId = x.StoreId,
                    CategoryId = x.CategoryId,
                    UnitId = x.UnitId,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    DescriptionAr = x.DescriptionAr,
                    DescriptionEn = x.DescriptionEn,
                    Image = x.Image,
                    ProductType = x.ProductType,
                    PurchaseInputMode = x.PurchaseInputMode,
                    AllowDecimalQuantity = x.AllowDecimalQuantity,
                    Price = x.Price,
                    SalePrice = x.SalePrice,
                    CostPrice = x.CostPrice,
                    StockQuantity = x.StockQuantity,
                    MinStockQuantity = x.MinStockQuantity,
                    MinPurchaseQuantity = x.MinPurchaseQuantity,
                    MaxPurchaseQuantity = x.MaxPurchaseQuantity,
                    QuantityStep = x.QuantityStep,
                    Status = x.Status,
                    AdminNote = x.AdminNote,
                    VendorName = x.Vendor.User.FullName,
                    StoreNameAr = x.Store.NameAr,
                    StoreNameEn = x.Store.NameEn,
                    CategoryNameAr = x.Category.NameAr,
                    CategoryNameEn = x.Category.NameEn,
                    UnitNameAr = x.Unit != null ? x.Unit.NameAr : null,
                    RequestedAt = x.RequestedAt,
                    ReviewedAt = x.ReviewedAt,
                    ReviewedByUserId = x.ReviewedByUserId,
                    ProductId = x.ProductId
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<AdminProductRequestDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.ProductRequests
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new AdminProductRequestDto
                {
                    Id = x.Id,
                    VendorId = x.VendorId,
                    StoreId = x.StoreId,
                    CategoryId = x.CategoryId,
                    UnitId = x.UnitId,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    DescriptionAr = x.DescriptionAr,
                    DescriptionEn = x.DescriptionEn,
                    Image = x.Image,
                    ProductType = x.ProductType,
                    PurchaseInputMode = x.PurchaseInputMode,
                    AllowDecimalQuantity = x.AllowDecimalQuantity,
                    Price = x.Price,
                    SalePrice = x.SalePrice,
                    CostPrice = x.CostPrice,
                    StockQuantity = x.StockQuantity,
                    MinStockQuantity = x.MinStockQuantity,
                    MinPurchaseQuantity = x.MinPurchaseQuantity,
                    MaxPurchaseQuantity = x.MaxPurchaseQuantity,
                    QuantityStep = x.QuantityStep,
                    Status = x.Status,
                    AdminNote = x.AdminNote,
                    VendorName = x.Vendor.User.FullName,
                    StoreNameAr = x.Store.NameAr,
                    StoreNameEn = x.Store.NameEn,
                    CategoryNameAr = x.Category.NameAr,
                    CategoryNameEn = x.Category.NameEn,
                    UnitNameAr = x.Unit != null ? x.Unit.NameAr : null,
                    RequestedAt = x.RequestedAt,
                    ReviewedAt = x.ReviewedAt,
                    ReviewedByUserId = x.ReviewedByUserId,
                    ProductId = x.ProductId
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<AdminProductRequestDto> ApproveAsync(
            int productRequestId,
            int adminUserId,
            CancellationToken cancellationToken = default)
        {
            var pr = await _context.ProductRequests
                .Include(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == productRequestId, cancellationToken)
                ?? throw new KeyNotFoundException("طلب المنتج غير موجود.");

            if (pr.Status != ProductApprovalStatus.Pending)
                throw new InvalidOperationException($"لا يمكن الموافقة على الطلب؛ حالته: {pr.Status}.");

            var createDto = new CreateAdminProductDto
            {
                CategoryId = pr.CategoryId,
                UnitId = pr.UnitId,
                NameAr = pr.NameAr,
                NameEn = pr.NameEn,
                DescriptionAr = pr.DescriptionAr,
                DescriptionEn = pr.DescriptionEn,
                ImageBase64 = pr.Image != null ? Convert.ToBase64String(pr.Image) : null,
                ProductType = ProductType.Simple,
                PurchaseInputMode = pr.PurchaseInputMode,
                AllowDecimalQuantity = pr.AllowDecimalQuantity,
                Price = pr.Price,
                SalePrice = pr.SalePrice,
                CostPrice = pr.CostPrice,
                StockQuantity = pr.StockQuantity,
                MinStockQuantity = pr.MinStockQuantity,
                MinPurchaseQuantity = pr.MinPurchaseQuantity,
                MaxPurchaseQuantity = pr.MaxPurchaseQuantity,
                QuantityStep = pr.QuantityStep,
                IsActive = true
            };

            var product = await _productService.CreateAsync(createDto, cancellationToken);

            pr.ProductId = product.Id;
            pr.Status = ProductApprovalStatus.Approved;
            pr.ReviewedAt = DateTime.UtcNow;
            pr.ReviewedByUserId = adminUserId;
            await _context.SaveChangesAsync(cancellationToken);

            return (await GetByIdAsync(pr.Id, cancellationToken))!;
        }

        public async Task<AdminProductRequestDto> RejectAsync(
            int productRequestId,
            int adminUserId,
            RejectProductRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var pr = await _context.ProductRequests
                .FirstOrDefaultAsync(x => x.Id == productRequestId, cancellationToken)
                ?? throw new KeyNotFoundException("طلب المنتج غير موجود.");

            if (pr.Status != ProductApprovalStatus.Pending)
                throw new InvalidOperationException($"لا يمكن رفض الطلب؛ حالته: {pr.Status}.");

            pr.Status = ProductApprovalStatus.Rejected;
            pr.AdminNote = dto?.AdminNote?.Trim();
            pr.ReviewedAt = DateTime.UtcNow;
            pr.ReviewedByUserId = adminUserId;
            await _context.SaveChangesAsync(cancellationToken);

            return (await GetByIdAsync(pr.Id, cancellationToken))!;
        }
    }
}
