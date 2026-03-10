using MarcketPlace.Application.Customer.Stores.Dtos;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Customer.Stores
{
    public class CustomerStoreCatalogService : ICustomerStoreCatalogService
    {
        private readonly AppDbContext _context;

        public CustomerStoreCatalogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<CustomerStoreProductDto>> GetStoreProductsAsync(
            int storeId,
            int? categoryId,
            CancellationToken cancellationToken = default)
        {
            var storeExists = await _context.Stores
                .AsNoTracking()
                .AnyAsync(x => x.Id == storeId && x.IsActive, cancellationToken);

            if (!storeExists)
                throw new KeyNotFoundException("المتجر غير موجود أو غير نشط.");

            HashSet<int>? categoryIds = null;

            if (categoryId.HasValue)
            {
                categoryIds = await GetCategoryWithDescendantsAsync(categoryId.Value, cancellationToken);

                if (categoryIds.Count == 0)
                    throw new KeyNotFoundException("التصنيف غير موجود.");
            }

            var query = _context.Products
                .AsNoTracking()
                .Where(x =>
                    x.StoreId == storeId &&
                    x.ApprovalStatus == ProductApprovalStatus.Approved &&
                    x.StockQuantity > 0);

            if (categoryIds is not null)
                query = query.Where(x => categoryIds.Contains(x.CategoryId));

            var products = await query
                .OrderBy(x => x.NameAr)
                .Select(x => new CustomerStoreProductDto
                {
                    Id = x.Id,
                    StoreId = x.StoreId!.Value,
                    CategoryId = x.CategoryId,
                    NameAr = x.NameAr,
                    NameEn = x.NameEn,
                    DescriptionAr = x.DescriptionAr,
                    DescriptionEn = x.DescriptionEn,
                    ImageUrl = x.ImageUrl,
                    Price = x.Price,
                    StockQuantity = x.StockQuantity,
                    CategoryNameAr = x.Category.NameAr,
                    CategoryNameEn = x.Category.NameEn
                })
                .ToListAsync(cancellationToken);

            return products;
        }

        private async Task<HashSet<int>> GetCategoryWithDescendantsAsync(
            int categoryId,
            CancellationToken cancellationToken)
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Select(x => new { x.Id, x.ParentId })
                .ToListAsync(cancellationToken);

            if (!categories.Any(x => x.Id == categoryId))
                return new HashSet<int>();

            var result = new HashSet<int> { categoryId };
            var queue = new Queue<int>();
            queue.Enqueue(categoryId);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();

                var children = categories
                    .Where(x => x.ParentId == currentId)
                    .Select(x => x.Id)
                    .ToList();

                foreach (var childId in children)
                {
                    if (result.Add(childId))
                        queue.Enqueue(childId);
                }
            }

            return result;
        }
    }
}