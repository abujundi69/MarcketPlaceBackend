using MarcketPlace.Application.Customer.Favorites.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Customer.Favorites
{
    public class CustomerFavoriteService : ICustomerFavoriteService
    {
        private readonly AppDbContext _context;

        public CustomerFavoriteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<CustomerFavoriteProductDto>> GetMyFavoritesAsync(
            int customerUserId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var favorites = await _context.CustomerFavorites
                .AsNoTracking()
                .Include(x => x.Product)
                    .ThenInclude(x => x.Unit)
                .Include(x => x.Product)
                    .ThenInclude(x => x.Category)
                .Include(x => x.Product)
                    .ThenInclude(x => x.Variants)
                        .ThenInclude(v => v.Unit)
                .Where(x =>
                    x.CustomerId == customerId &&
                    x.Product.IsActive &&
                    x.Product.Category.IsActive)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            return favorites.Select(MapFavorite).ToList();
        }

        public async Task<CustomerFavoriteToggleResultDto> AddAsync(
            int customerUserId,
            int productId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);
            await EnsureProductExistsAsync(productId, cancellationToken);

            var exists = await _context.CustomerFavorites
                .AnyAsync(x => x.CustomerId == customerId && x.ProductId == productId, cancellationToken);

            if (!exists)
            {
                _context.CustomerFavorites.Add(new CustomerFavorite
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync(cancellationToken);
            }

            return new CustomerFavoriteToggleResultDto
            {
                ProductId = productId,
                IsFavorite = true
            };
        }

        public async Task<CustomerFavoriteToggleResultDto> RemoveAsync(
            int customerUserId,
            int productId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);

            var favorite = await _context.CustomerFavorites
                .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.ProductId == productId, cancellationToken);

            if (favorite is not null)
            {
                _context.CustomerFavorites.Remove(favorite);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return new CustomerFavoriteToggleResultDto
            {
                ProductId = productId,
                IsFavorite = false
            };
        }

        public async Task<CustomerFavoriteToggleResultDto> ToggleAsync(
            int customerUserId,
            int productId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(customerUserId, cancellationToken);
            await EnsureProductExistsAsync(productId, cancellationToken);

            var favorite = await _context.CustomerFavorites
                .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.ProductId == productId, cancellationToken);

            if (favorite is null)
            {
                _context.CustomerFavorites.Add(new CustomerFavorite
                {
                    CustomerId = customerId,
                    ProductId = productId,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync(cancellationToken);

                return new CustomerFavoriteToggleResultDto
                {
                    ProductId = productId,
                    IsFavorite = true
                };
            }

            _context.CustomerFavorites.Remove(favorite);
            await _context.SaveChangesAsync(cancellationToken);

            return new CustomerFavoriteToggleResultDto
            {
                ProductId = productId,
                IsFavorite = false
            };
        }

        private async Task<int> GetCustomerIdAsync(
            int customerUserId,
            CancellationToken cancellationToken)
        {
            var customerId = await _context.Customers
                .Where(x => x.UserId == customerUserId)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (!customerId.HasValue)
                throw new KeyNotFoundException("الزبون غير موجود.");

            return customerId.Value;
        }

        private async Task EnsureProductExistsAsync(
            int productId,
            CancellationToken cancellationToken)
        {
            var exists = await _context.Products
                .AnyAsync(x =>
                    x.Id == productId &&
                    x.IsActive &&
                    x.Category.IsActive,
                    cancellationToken);

            if (!exists)
                throw new KeyNotFoundException("المنتج غير موجود.");
        }

        private CustomerFavoriteProductDto MapFavorite(CustomerFavorite favorite)
        {
            var product = favorite.Product;
            var defaultVariant = GetDefaultVariant(product);

            var purchaseInputMode = defaultVariant?.PurchaseInputModeOverride ?? product.PurchaseInputMode;
            var allowDecimalQuantity = defaultVariant?.AllowDecimalQuantityOverride ?? product.AllowDecimalQuantity;

            var price = defaultVariant?.Price ?? product.Price;
            var salePrice = defaultVariant?.SalePrice ?? product.SalePrice;
            var effectivePrice = salePrice ?? price;
            var unitSymbol = defaultVariant?.Unit?.Symbol ?? product.Unit?.Symbol;

            var isAvailable = product.ProductType == ProductType.Variable
                ? product.Variants.Any(v => v.IsActive && v.StockQuantity > 0)
                : product.StockQuantity > 0;

            return new CustomerFavoriteProductDto
            {
                ProductId = product.Id,
                CategoryId = product.CategoryId,
                NameAr = product.NameAr,
                NameEn = product.NameEn,
                Image = product.Image,
                ProductType = product.ProductType,
                PurchaseInputMode = purchaseInputMode,
                AllowDecimalQuantity = allowDecimalQuantity,
                DefaultVariantId = defaultVariant?.Id,
                DefaultVariantNameAr = defaultVariant?.NameAr,
                DefaultVariantNameEn = defaultVariant?.NameEn,
                UnitSymbol = unitSymbol,
                Price = price,
                SalePrice = salePrice,
                EffectivePrice = effectivePrice,
                IsAvailable = isAvailable,
                AddedAt = favorite.CreatedAt
            };
        }

        private ProductVariant? GetDefaultVariant(Product product)
        {
            if (product.ProductType != ProductType.Variable)
                return null;

            return product.Variants
                .Where(v => v.IsActive)
                .OrderByDescending(v => v.IsDefault)
                .ThenBy(v => v.SortOrder)
                .FirstOrDefault();
        }
    }
}