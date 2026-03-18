using MarcketPlace.Application.Vendor.Products.Dtos;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Vendor.Products
{
    public class VendorProductService : IVendorProductService
    {
        private readonly AppDbContext _context;

        public VendorProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<VendorProductDto>> GetMyProductsAsync(int vendorUserId, CancellationToken cancellationToken = default)
        {
            var vendor = await _context.Vendors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == vendorUserId, cancellationToken);

            if (vendor is null)
                return Array.Empty<VendorProductDto>();

            var productIds = await _context.ProductRequests
                .AsNoTracking()
                .Where(x => x.VendorId == vendor.Id && x.Status == ProductApprovalStatus.Approved && x.ProductId != null)
                .Select(x => x.ProductId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (productIds.Count == 0)
                return Array.Empty<VendorProductDto>();

            var prByProduct = await _context.ProductRequests
                .AsNoTracking()
                .Where(x => x.VendorId == vendor.Id && x.ProductId != null && productIds.Contains(x.ProductId.Value))
                .Select(x => new { x.ProductId, x.StoreId })
                .ToListAsync(cancellationToken);

            var productStoreMap = prByProduct.ToDictionary(x => x.ProductId!.Value, x => x.StoreId);

            var products = await _context.Products
                .AsNoTracking()
                .Where(x => productIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.CategoryId,
                    x.NameAr,
                    x.NameEn,
                    x.DescriptionAr,
                    x.DescriptionEn,
                    x.Image,
                    x.Price,
                    x.StockQuantity,
                    x.MinStockQuantity,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return products
                .Where(x => productStoreMap.ContainsKey(x.Id))
                .Select(x => new VendorProductDto
                {
                    Id = x.Id,
                    StoreId = productStoreMap[x.Id],
                    CategoryId = x.CategoryId,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    DescriptionAr = x.DescriptionAr,
                    DescriptionEn = x.DescriptionEn,
                    Image = x.Image != null ? Convert.ToBase64String(x.Image) : null,
                    Price = x.Price,
                    StockQuantity = x.StockQuantity,
                    MinStockQuantity = x.MinStockQuantity,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
        }

        public async Task<VendorProductDto?> GetByIdAsync(int vendorUserId, int productId, CancellationToken cancellationToken = default)
        {
            var vendor = await _context.Vendors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == vendorUserId, cancellationToken);

            if (vendor is null)
                return null;

            var pr = await _context.ProductRequests
                .AsNoTracking()
                .Where(x => x.VendorId == vendor.Id && x.ProductId == productId && x.Status == ProductApprovalStatus.Approved)
                .Select(x => new { x.StoreId })
                .FirstOrDefaultAsync(cancellationToken);

            if (pr is null)
                return null;

            var product = await _context.Products
                .AsNoTracking()
                .Where(x => x.Id == productId)
                .Select(x => new
                {
                    x.Id,
                    x.CategoryId,
                    x.NameAr,
                    x.NameEn,
                    x.DescriptionAr,
                    x.DescriptionEn,
                    x.Image,
                    x.Price,
                    x.StockQuantity,
                    x.MinStockQuantity,
                    x.CreatedAt,
                    x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (product is null)
                return null;

            return new VendorProductDto
            {
                Id = product.Id,
                StoreId = pr.StoreId,
                CategoryId = product.CategoryId,
                NameAr = product.NameAr,
                NameEn = product.NameEn,
                DescriptionAr = product.DescriptionAr,
                DescriptionEn = product.DescriptionEn,
                Image = product.Image != null ? Convert.ToBase64String(product.Image) : null,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                MinStockQuantity = product.MinStockQuantity,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
