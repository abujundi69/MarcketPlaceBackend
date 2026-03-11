using MarcketPlace.Application.Vendor.Products.Dtos;
using MarcketPlace.Domain.Entities;
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

        public async Task<IReadOnlyList<VendorProductDto>> GetAllByUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var vendorId = await GetVendorIdByUserIdAsync(userId, cancellationToken);

            return await _context.Products
                .AsNoTracking()
                .Where(x => x.Store != null && x.Store.VendorId == vendorId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new VendorProductDto
                {
                    Id = x.Id,
                    StoreId = x.StoreId,
                    CategoryId = x.CategoryId,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    DescriptionAr = x.DescriptionAr,
                    DescriptionEn = x.DescriptionEn,
                    Image = x.Image,
                    Price = x.Price,
                    StockQuantity = x.StockQuantity,
                    MinStockQuantity = x.MinStockQuantity,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<VendorProductDto> GetByIdByUserAsync(
            int userId,
            int productId,
            CancellationToken cancellationToken = default)
        {
            var vendorId = await GetVendorIdByUserIdAsync(userId, cancellationToken);

            var product = await _context.Products
                .AsNoTracking()
                .Include(x => x.Store)
                .FirstOrDefaultAsync(
                    x => x.Id == productId && x.Store != null && x.Store.VendorId == vendorId,
                    cancellationToken);

            if (product is null)
                throw new KeyNotFoundException("المنتج غير موجود أو لا يتبع لهذا التاجر.");

            return MapToDto(product);
        }

        public async Task<VendorProductDto> UpdateByUserAsync(
            int userId,
            int productId,
            UpdateVendorProductDto dto,
            CancellationToken cancellationToken = default)
        {
            var vendorId = await GetVendorIdByUserIdAsync(userId, cancellationToken);
            return await UpdateAsync(vendorId, productId, dto, cancellationToken);
        }

        private async Task<VendorProductDto> UpdateAsync(
            int vendorId,
            int productId,
            UpdateVendorProductDto dto,
            CancellationToken cancellationToken = default)
        {
            var vendor = await _context.Vendors
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == vendorId, cancellationToken);

            if (vendor is null)
                throw new KeyNotFoundException("التاجر غير موجود.");

            if (!vendor.IsApproved)
                throw new InvalidOperationException("لا يمكن تعديل المنتجات قبل اعتماد حساب التاجر.");

            if (dto.Price < 0)
                throw new ArgumentException("السعر لا يمكن أن يكون أقل من صفر.");

            if (dto.StockQuantity < 0)
                throw new ArgumentException("الكمية لا يمكن أن تكون أقل من صفر.");

            var product = await _context.Products
                .Include(x => x.Store)
                .FirstOrDefaultAsync(
                    x => x.Id == productId && x.Store != null && x.Store.VendorId == vendorId,
                    cancellationToken);

            if (product is null)
                throw new KeyNotFoundException("المنتج غير موجود أو لا يتبع لهذا التاجر.");

            if (product.Store is null || !product.Store.IsActive)
                throw new InvalidOperationException("لا يمكن تعديل منتج داخل متجر غير نشط.");

            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return MapToDto(product);
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
                throw new KeyNotFoundException("المستخدم الحالي لا يملك حساب تاجر.");

            return vendorId.Value;
        }

        private static VendorProductDto MapToDto(Product product)
        {
            return new VendorProductDto
            {
                Id = product.Id,
                StoreId = product.StoreId,
                CategoryId = product.CategoryId,
                NameAr = product.NameAr,
                NameEn = product.NameEn,
                DescriptionAr = product.DescriptionAr,
                DescriptionEn = product.DescriptionEn,
                Image = product.Image,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                MinStockQuantity = product.MinStockQuantity,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}