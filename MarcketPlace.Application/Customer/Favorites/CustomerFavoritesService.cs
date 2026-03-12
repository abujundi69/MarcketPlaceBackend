using MarcketPlace.Application.Customer.Favorites.Dtos;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Customer.Favorites
{
    public class CustomerFavoritesService : ICustomerFavoritesService
    {
        private readonly AppDbContext _context;

        public CustomerFavoritesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<CustomerFavoriteProductDto>> GetFavoritesAsync(
            int customerUserId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await TryGetCustomerIdAsync(customerUserId, cancellationToken);
            if (customerId is null)
                return Array.Empty<CustomerFavoriteProductDto>();

            var products = await _context.CustomerFavorites
                .AsNoTracking()
                .Where(x => x.CustomerId == customerId.Value)
                .Include(x => x.Product)
                    .ThenInclude(x => x!.Category)
                .Where(x => x.Product != null &&
                            x.Product.ApprovalStatus == ProductApprovalStatus.Approved &&
                            x.Product.StockQuantity > 0)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new CustomerFavoriteProductDto
                {
                    Id = x.Product!.Id,
                    StoreId = x.Product.StoreId ?? 0,
                    CategoryId = x.Product.CategoryId,
                    NameAr = x.Product.NameAr,
                    NameEn = x.Product.NameEn,
                    DescriptionAr = x.Product.DescriptionAr,
                    DescriptionEn = x.Product.DescriptionEn,
                    Image = x.Product.Image,
                    Price = x.Product.Price,
                    StockQuantity = x.Product.StockQuantity,
                    CategoryNameAr = x.Product.Category.NameAr,
                    CategoryNameEn = x.Product.Category.NameEn
                })
                .ToListAsync(cancellationToken);

            return products;
        }

        public async Task AddFavoriteAsync(
            int customerUserId,
            int productId,
            CancellationToken cancellationToken = default)
        {
            if (productId <= 0)
                throw new InvalidOperationException("المنتج غير صالح.");

            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var product = await _context.Products
                .Include(x => x.Store)
                .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);

            if (product is null)
                throw new KeyNotFoundException("المنتج غير موجود.");

            if (product.ApprovalStatus != ProductApprovalStatus.Approved)
                throw new InvalidOperationException("المنتج غير معتمد.");

            if (product.StockQuantity <= 0)
                throw new InvalidOperationException("المنتج غير متوفر حاليًا.");

            var exists = await _context.CustomerFavorites
                .AnyAsync(x => x.CustomerId == customerId && x.ProductId == productId, cancellationToken);

            if (exists)
                return;

            _context.CustomerFavorites.Add(new Domain.Entities.CustomerFavorite
            {
                CustomerId = customerId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveFavoriteAsync(
            int customerUserId,
            int productId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var favorite = await _context.CustomerFavorites
                .FirstOrDefaultAsync(
                    x => x.CustomerId == customerId && x.ProductId == productId,
                    cancellationToken);

            if (favorite is null)
                return;

            _context.CustomerFavorites.Remove(favorite);
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<int?> TryGetCustomerIdAsync(int customerUserId, CancellationToken cancellationToken)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == customerUserId, cancellationToken);
            return customer?.Id;
        }

        private async Task<int> GetCustomerIdAsync(int customerUserId, CancellationToken cancellationToken)
        {
            var customer = await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == customerUserId, cancellationToken);

            if (customer is null)
                throw new KeyNotFoundException("الزبون غير موجود.");

            return customer.Id;
        }
    }
}
