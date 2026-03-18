using MarcketPlace.Application.Customer.Catalog.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Customer.Catalog
{
    public class CustomerCatalogService : ICustomerCatalogService
    {
        private readonly AppDbContext _context;

        public CustomerCatalogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<CustomerCategoryDto>> GetCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            var categories = await _context.Categories
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.NameAr)
                .Select(x => new { x.Id, x.NameAr, x.NameEn, x.Image, x.DisplayOrder, x.ParentId })
                .ToListAsync(cancellationToken);

            if (categories.Count == 0)
                return Array.Empty<CustomerCategoryDto>();

            var categoryIds = categories.Select(c => c.Id).ToList();
            var counts = await _context.Products
                .AsNoTracking()
                .Where(p => categoryIds.Contains(p.CategoryId) && p.IsActive)
                .GroupBy(p => p.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            var countDict = counts.ToDictionary(x => x.CategoryId, x => x.Count);

            return categories.Select(c => new CustomerCategoryDto
            {
                Id = c.Id,
                NameAr = c.NameAr,
                NameEn = c.NameEn,
                Image = c.Image,
                DisplayOrder = c.DisplayOrder,
                ParentId = c.ParentId,
                ProductsCount = countDict.TryGetValue(c.Id, out var cnt) ? cnt : 0
            }).ToList();
        }

        public async Task<IReadOnlyList<CustomerProductListItemDto>> GetProductsByCategoryAsync(
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            var categoryExists = await _context.Categories
                .AsNoTracking()
                .AnyAsync(x => x.Id == categoryId && x.IsActive, cancellationToken);

            if (!categoryExists)
                throw new KeyNotFoundException("الكاتيجوري غير موجودة.");

            var products = await _context.Products
                .AsNoTracking()
                .Include(x => x.Unit)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Unit)
                .Where(x => x.CategoryId == categoryId && x.IsActive)
                .OrderBy(x => x.NameAr)
                .ToListAsync(cancellationToken);

            return products.Select(MapProductListItem).ToList();
        }

        public async Task<CustomerProductDetailsDto> GetProductDetailsAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(x => x.Unit)
                .Include(x => x.Category)
                .Include(x => x.Options)
                    .ThenInclude(o => o.Values)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Unit)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.VariantOptionValues)
                        .ThenInclude(z => z.ProductOptionValue)
                            .ThenInclude(v => v.ProductOption)
                .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);

            if (product is null ||
                !product.IsActive ||
                product.Category is null ||
                !product.Category.IsActive)
            {
                throw new KeyNotFoundException("المنتج غير موجود.");
            }

            var defaultVariant = GetDefaultVariant(product);
            var effectivePurchaseMode = GetEffectivePurchaseMode(product, defaultVariant);
            var effectiveAllowDecimalQuantity = GetEffectiveAllowDecimalQuantity(product, defaultVariant);

            return new CustomerProductDetailsDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                NameAr = product.NameAr,
                NameEn = product.NameEn,
                DescriptionAr = product.DescriptionAr,
                DescriptionEn = product.DescriptionEn,
                Image = product.Image,
                ProductType = product.ProductType,
                PurchaseInputMode = effectivePurchaseMode,
                AllowDecimalQuantity = effectiveAllowDecimalQuantity,

                UnitId = product.UnitId,
                UnitNameAr = product.Unit?.NameAr,
                UnitNameEn = product.Unit?.NameEn,
                UnitSymbol = defaultVariant?.Unit?.Symbol ?? product.Unit?.Symbol,

                Price = defaultVariant?.Price ?? product.Price,
                SalePrice = defaultVariant?.SalePrice ?? product.SalePrice,
                EffectivePrice = (defaultVariant?.SalePrice ?? defaultVariant?.Price)
                                 ?? (product.SalePrice ?? product.Price),

                StockQuantity = product.ProductType == ProductType.Variable
                    ? product.Variants.Where(v => v.IsActive).Sum(v => v.StockQuantity)
                    : product.StockQuantity,

                MinStockQuantity = product.ProductType == ProductType.Variable
                    ? product.Variants.Where(v => v.IsActive).Sum(v => v.MinStockQuantity)
                    : product.MinStockQuantity,

                MinPurchaseQuantity = defaultVariant?.MinPurchaseQuantity ?? product.MinPurchaseQuantity,
                MaxPurchaseQuantity = defaultVariant?.MaxPurchaseQuantity ?? product.MaxPurchaseQuantity,
                QuantityStep = defaultVariant?.QuantityStep ?? product.QuantityStep,
                DefaultVariantId = defaultVariant?.Id,

                Options = product.Options
                    .OrderBy(o => o.SortOrder)
                    .Select(o => new CustomerProductOptionDto
                    {
                        Id = o.Id,
                        NameAr = o.NameAr,
                        NameEn = o.NameEn,
                        SortOrder = o.SortOrder,
                        Values = o.Values
                            .OrderBy(v => v.SortOrder)
                            .Select(v => new CustomerProductOptionValueDto
                            {
                                Id = v.Id,
                                ValueAr = v.ValueAr,
                                ValueEn = v.ValueEn,
                                ColorHex = v.ColorHex,
                                SortOrder = v.SortOrder
                            })
                            .ToList()
                    })
                    .ToList(),

                Variants = product.Variants
                    .Where(v => v.IsActive)
                    .OrderByDescending(v => v.IsDefault)
                    .ThenBy(v => v.SortOrder)
                    .Select(v => new CustomerProductVariantDto
                    {
                        Id = v.Id,
                        NameAr = v.NameAr,
                        NameEn = v.NameEn,
                        UnitId = v.UnitId,
                        UnitNameAr = v.Unit?.NameAr,
                        UnitNameEn = v.Unit?.NameEn,
                        UnitSymbol = v.Unit?.Symbol ?? product.Unit?.Symbol,
                        Price = v.Price,
                        SalePrice = v.SalePrice,
                        EffectivePrice = v.SalePrice ?? v.Price,
                        StockQuantity = v.StockQuantity,
                        MinStockQuantity = v.MinStockQuantity,
                        MinPurchaseQuantity = v.MinPurchaseQuantity,
                        MaxPurchaseQuantity = v.MaxPurchaseQuantity,
                        QuantityStep = v.QuantityStep,
                        PurchaseInputMode = GetEffectivePurchaseMode(product, v),
                        AllowDecimalQuantity = GetEffectiveAllowDecimalQuantity(product, v),
                        IsDefault = v.IsDefault,
                        IsActive = v.IsActive,
                        SortOrder = v.SortOrder,
                        SelectedValues = v.VariantOptionValues
                            .OrderBy(z => z.ProductOptionValue.ProductOption.SortOrder)
                            .ThenBy(z => z.ProductOptionValue.SortOrder)
                            .Select(z => new CustomerProductVariantSelectedValueDto
                            {
                                OptionId = z.ProductOptionValue.ProductOptionId,
                                OptionNameAr = z.ProductOptionValue.ProductOption.NameAr,
                                OptionNameEn = z.ProductOptionValue.ProductOption.NameEn,
                                OptionValueId = z.ProductOptionValueId,
                                ValueAr = z.ProductOptionValue.ValueAr,
                                ValueEn = z.ProductOptionValue.ValueEn,
                                ColorHex = z.ProductOptionValue.ColorHex
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }

        public async Task<IReadOnlyList<CustomerMostOrderedProductDto>> GetMostOrderedProductsAsync(
            int take = 10,
            CancellationToken cancellationToken = default)
        {
            take = take <= 0 ? 10 : Math.Min(take, 50);

            var stats = await _context.OrderItems
                .AsNoTracking()
                .Where(x =>
                    x.Order.CancelledAt == null &&
                    x.Order.Status != OrderStatus.Cancelled)
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    OrdersCount = g.Select(x => x.OrderId).Distinct().Count(),
                    TotalOrderedQuantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.OrdersCount)
                .ThenByDescending(x => x.TotalOrderedQuantity)
                .Take(take)
                .ToListAsync(cancellationToken);

            if (stats.Count == 0)
                return Array.Empty<CustomerMostOrderedProductDto>();

            var productIds = stats.Select(x => x.ProductId).ToList();

            var products = await _context.Products
                .AsNoTracking()
                .Include(x => x.Unit)
                .Include(x => x.Category)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Unit)
                .Where(x =>
                    productIds.Contains(x.Id) &&
                    x.IsActive &&
                    x.Category.IsActive)
                .ToListAsync(cancellationToken);

            var productsMap = products.ToDictionary(x => x.Id);
            var result = new List<CustomerMostOrderedProductDto>();

            foreach (var stat in stats)
            {
                if (!productsMap.TryGetValue(stat.ProductId, out var product))
                    continue;

                var defaultVariant = GetDefaultVariant(product);
                var effectivePurchaseMode = GetEffectivePurchaseMode(product, defaultVariant);
                var effectiveAllowDecimalQuantity = GetEffectiveAllowDecimalQuantity(product, defaultVariant);

                var price = defaultVariant?.Price ?? product.Price;
                var salePrice = defaultVariant?.SalePrice ?? product.SalePrice;
                var effectivePrice = salePrice ?? price;
                var unitSymbol = defaultVariant?.Unit?.Symbol ?? product.Unit?.Symbol;

                var isAvailable = product.ProductType == ProductType.Variable
                    ? product.Variants.Any(v => v.IsActive && v.StockQuantity > 0)
                    : product.StockQuantity > 0;

                result.Add(new CustomerMostOrderedProductDto
                {
                    ProductId = product.Id,
                    DefaultVariantId = defaultVariant?.Id,
                    NameAr = product.NameAr,
                    NameEn = product.NameEn,
                    Image = product.Image,
                    ProductType = product.ProductType,
                    PurchaseInputMode = effectivePurchaseMode,
                    AllowDecimalQuantity = effectiveAllowDecimalQuantity,
                    DefaultVariantNameAr = defaultVariant?.NameAr,
                    DefaultVariantNameEn = defaultVariant?.NameEn,
                    UnitSymbol = unitSymbol,
                    Price = price,
                    SalePrice = salePrice,
                    EffectivePrice = effectivePrice,
                    IsAvailable = isAvailable,
                    OrdersCount = stat.OrdersCount,
                    TotalOrderedQuantity = stat.TotalOrderedQuantity
                });
            }

            return result;
        }

        private CustomerProductListItemDto MapProductListItem(Product product)
        {
            var defaultVariant = GetDefaultVariant(product);
            var effectivePurchaseMode = GetEffectivePurchaseMode(product, defaultVariant);
            var effectiveAllowDecimalQuantity = GetEffectiveAllowDecimalQuantity(product, defaultVariant);

            var price = defaultVariant?.Price ?? product.Price;
            var salePrice = defaultVariant?.SalePrice ?? product.SalePrice;
            var effectivePrice = salePrice ?? price;
            var unitSymbol = defaultVariant?.Unit?.Symbol ?? product.Unit?.Symbol;

            var isAvailable = product.ProductType == ProductType.Variable
                ? product.Variants.Any(v => v.IsActive && v.StockQuantity > 0)
                : product.StockQuantity > 0;

            return new CustomerProductListItemDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                NameAr = product.NameAr,
                NameEn = product.NameEn,
                Image = product.Image,
                ProductType = product.ProductType,
                PurchaseInputMode = effectivePurchaseMode,
                AllowDecimalQuantity = effectiveAllowDecimalQuantity,
                DefaultVariantId = defaultVariant?.Id,
                DefaultVariantNameAr = defaultVariant?.NameAr,
                DefaultVariantNameEn = defaultVariant?.NameEn,
                UnitSymbol = unitSymbol,
                Price = price,
                SalePrice = salePrice,
                EffectivePrice = effectivePrice,
                IsAvailable = isAvailable
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

        private static ProductPurchaseInputMode GetEffectivePurchaseMode(Product product, ProductVariant? variant)
        {
            return variant?.PurchaseInputModeOverride ?? product.PurchaseInputMode;
        }

        private static bool GetEffectiveAllowDecimalQuantity(Product product, ProductVariant? variant)
        {
            return variant?.AllowDecimalQuantityOverride ?? product.AllowDecimalQuantity;
        }
    }
}