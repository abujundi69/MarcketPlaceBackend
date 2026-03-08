using MarcketPlace.Application.Admin.Products.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.Products
{
    public class ProductAdminService : IProductAdminService
    {
        private readonly AppDbContext _context;

        public ProductAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
        {
            var nameAr = dto.NameAr?.Trim();
            var nameEn = dto.NameEn?.Trim();

            if (string.IsNullOrWhiteSpace(nameAr))
                throw new Exception("الاسم العربي مطلوب.");

            if (string.IsNullOrWhiteSpace(nameEn))
                throw new Exception("الاسم الإنجليزي مطلوب.");

            if (dto.Price < 0)
                throw new Exception("السعر لا يمكن أن يكون أقل من صفر.");

            if (dto.StockQuantity < 0)
                throw new Exception("الكمية المتوفرة لا يمكن أن تكون أقل من صفر.");

            if (dto.MinStockQuantity < 0)
                throw new Exception("الحد الأدنى للكمية لا يمكن أن يكون أقل من صفر.");

            int? storeId = dto.StoreId;
            if (storeId <= 0)
                storeId = null;

            var categoryExists = await _context.Categories
                .AnyAsync(x => x.Id == dto.CategoryId, cancellationToken);

            if (!categoryExists)
                throw new Exception("التصنيف غير موجود.");

            if (storeId.HasValue)
            {
                var storeExists = await _context.Stores
                    .AnyAsync(x => x.Id == storeId.Value, cancellationToken);

                if (!storeExists)
                    throw new Exception("المتجر غير موجود.");
            }

            var duplicateExists = await _context.Products
                .AnyAsync(x =>
                    x.CategoryId == dto.CategoryId &&
                    x.StoreId == storeId &&
                    (x.NameAr == nameAr || x.NameEn == nameEn),
                    cancellationToken);

            if (duplicateExists)
                throw new Exception("يوجد منتج بنفس الاسم العربي أو الإنجليزي ضمن نفس التصنيف والمتجر.");

            var product = new Product
            {
                StoreId = storeId,
                CategoryId = dto.CategoryId,
                NameAr = nameAr,
                NameEn = nameEn,
                DescriptionAr = dto.DescriptionAr?.Trim(),
                DescriptionEn = dto.DescriptionEn?.Trim(),
                ImageUrl = dto.ImageUrl?.Trim(),
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                MinStockQuantity = dto.MinStockQuantity,
                ApprovalStatus = dto.ApprovalStatus,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(product);
        }

        public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (product is null)
                return null;

            var nameAr = dto.NameAr?.Trim();
            var nameEn = dto.NameEn?.Trim();

            if (string.IsNullOrWhiteSpace(nameAr))
                throw new Exception("الاسم العربي مطلوب.");

            if (string.IsNullOrWhiteSpace(nameEn))
                throw new Exception("الاسم الإنجليزي مطلوب.");

            if (dto.Price < 0)
                throw new Exception("السعر لا يمكن أن يكون أقل من صفر.");

            if (dto.StockQuantity < 0)
                throw new Exception("الكمية المتوفرة لا يمكن أن تكون أقل من صفر.");

            if (dto.MinStockQuantity < 0)
                throw new Exception("الحد الأدنى للكمية لا يمكن أن يكون أقل من صفر.");

            int? storeId = dto.StoreId;
            if (storeId <= 0)
                storeId = null;

            var categoryExists = await _context.Categories
                .AnyAsync(x => x.Id == dto.CategoryId, cancellationToken);

            if (!categoryExists)
                throw new Exception("التصنيف غير موجود.");

            if (storeId.HasValue)
            {
                var storeExists = await _context.Stores
                    .AnyAsync(x => x.Id == storeId.Value, cancellationToken);

                if (!storeExists)
                    throw new Exception("المتجر غير موجود.");
            }

            var duplicateExists = await _context.Products
                .AnyAsync(x =>
                    x.Id != id &&
                    x.CategoryId == dto.CategoryId &&
                    x.StoreId == storeId &&
                    (x.NameAr == nameAr || x.NameEn == nameEn),
                    cancellationToken);

            if (duplicateExists)
                throw new Exception("يوجد منتج بنفس الاسم العربي أو الإنجليزي ضمن نفس التصنيف والمتجر.");

            product.StoreId = storeId;
            product.CategoryId = dto.CategoryId;
            product.NameAr = nameAr;
            product.NameEn = nameEn;
            product.DescriptionAr = dto.DescriptionAr?.Trim();
            product.DescriptionEn = dto.DescriptionEn?.Trim();
            product.ImageUrl = dto.ImageUrl?.Trim();
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.MinStockQuantity = dto.MinStockQuantity;
            product.ApprovalStatus = dto.ApprovalStatus;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(product);
        }

        public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ProductDto
                {
                    Id = x.Id,
                    StoreId = x.StoreId,
                    CategoryId = x.CategoryId,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    DescriptionAr = x.DescriptionAr,
                    DescriptionEn = x.DescriptionEn,
                    ImageUrl = x.ImageUrl,
                    Price = x.Price,
                    StockQuantity = x.StockQuantity,
                    MinStockQuantity = x.MinStockQuantity,
                    ApprovalStatus = x.ApprovalStatus,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<ProductDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ProductDto
                {
                    Id = x.Id,
                    StoreId = x.StoreId,
                    CategoryId = x.CategoryId,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    DescriptionAr = x.DescriptionAr,
                    DescriptionEn = x.DescriptionEn,
                    ImageUrl = x.ImageUrl,
                    Price = x.Price,
                    StockQuantity = x.StockQuantity,
                    MinStockQuantity = x.MinStockQuantity,
                    ApprovalStatus = x.ApprovalStatus,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ProductDto>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(x => x.CategoryId == categoryId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ProductDto
                {
                    Id = x.Id,
                    StoreId = x.StoreId,
                    CategoryId = x.CategoryId,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    DescriptionAr = x.DescriptionAr,
                    DescriptionEn = x.DescriptionEn,
                    ImageUrl = x.ImageUrl,
                    Price = x.Price,
                    StockQuantity = x.StockQuantity,
                    MinStockQuantity = x.MinStockQuantity,
                    ApprovalStatus = x.ApprovalStatus,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancellationToken);
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                StoreId = product.StoreId,
                CategoryId = product.CategoryId,
                NameAr = product.NameAr,
                NameEn = product.NameEn,
                DescriptionAr = product.DescriptionAr,
                DescriptionEn = product.DescriptionEn,
                ImageUrl = product.ImageUrl,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                MinStockQuantity = product.MinStockQuantity,
                ApprovalStatus = product.ApprovalStatus,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}