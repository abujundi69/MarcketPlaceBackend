using MarcketPlace.Application.Admin.Products.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.Products
{
    public class AdminProductService : IAdminProductService
    {
        private readonly AppDbContext _context;

        public AdminProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminProductDto> CreateAsync(
            CreateAdminProductDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            await ValidateReferencesAsync(dto, cancellationToken);
            ValidateProductPayload(dto);

            var imageBytes = ParseBase64Image(dto.ImageBase64);
            var now = DateTime.UtcNow;

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var product = BuildProductEntity(dto, imageBytes, now);

            _context.Products.Add(product);

            var optionValueMap = new Dictionary<string, ProductOptionValue>(StringComparer.OrdinalIgnoreCase);

            if (dto.ProductType == ProductType.Variable)
            {
                BuildOptions(product, dto, now, optionValueMap);
                BuildVariants(product, dto, now, optionValueMap);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return await GetByIdAsync(product.Id, cancellationToken);
        }

        public async Task<AdminProductDto> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            var item = await _context.Products
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new AdminProductDto
                {
                    Id = x.Id,
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

                    IsActive = x.IsActive,

                    CategoryNameAr = x.Category.NameAr,
                    CategoryNameEn = x.Category.NameEn,

                    UnitNameAr = x.Unit != null ? x.Unit.NameAr : null,
                    UnitNameEn = x.Unit != null ? x.Unit.NameEn : null,
                    UnitSymbol = x.Unit != null ? x.Unit.Symbol : null,

                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,

                    Options = x.Options
                        .OrderBy(o => o.SortOrder)
                        .Select(o => new AdminProductOptionDto
                        {
                            Id = o.Id,
                            NameAr = o.NameAr,
                            NameEn = o.NameEn,
                            SortOrder = o.SortOrder,
                            Values = o.Values
                                .OrderBy(v => v.SortOrder)
                                .Select(v => new AdminProductOptionValueDto
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

                    Variants = x.Variants
                        .OrderBy(v => v.SortOrder)
                        .Select(v => new AdminProductVariantDto
                        {
                            Id = v.Id,
                            UnitId = v.UnitId,
                            NameAr = v.NameAr,
                            NameEn = v.NameEn,
                            SKU = v.SKU,
                            Barcode = v.Barcode,
                            Price = v.Price,
                            SalePrice = v.SalePrice,
                            CostPrice = v.CostPrice,
                            StockQuantity = v.StockQuantity,
                            MinStockQuantity = v.MinStockQuantity,
                            MinPurchaseQuantity = v.MinPurchaseQuantity,
                            MaxPurchaseQuantity = v.MaxPurchaseQuantity,
                            QuantityStep = v.QuantityStep,
                            IsDefault = v.IsDefault,
                            IsActive = v.IsActive,
                            SortOrder = v.SortOrder,
                            UnitNameAr = v.Unit != null ? v.Unit.NameAr : null,
                            UnitNameEn = v.Unit != null ? v.Unit.NameEn : null,
                            UnitSymbol = v.Unit != null ? v.Unit.Symbol : null,
                            SelectedValues = v.VariantOptionValues
                                .OrderBy(z => z.ProductOptionValue.ProductOption.SortOrder)
                                .ThenBy(z => z.ProductOptionValue.SortOrder)
                                .Select(z => new AdminProductVariantSelectedValueDto
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
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (item is null)
                throw new KeyNotFoundException("المنتج غير موجود.");

            return item;
        }

        public async Task<IReadOnlyList<AdminProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var ids = await _context.Products
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var result = new List<AdminProductDto>();
            foreach (var id in ids)
            {
                result.Add(await GetByIdAsync(id, cancellationToken));
            }
            return result;
        }

        public async Task<IReadOnlyList<AdminProductDto>> GetByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            var ids = await _context.Products
                .AsNoTracking()
                .Where(x => x.CategoryId == categoryId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            var result = new List<AdminProductDto>();
            foreach (var id in ids)
            {
                result.Add(await GetByIdAsync(id, cancellationToken));
            }
            return result;
        }

        public async Task<AdminProductDto> UpdateAsync(int id, UpdateAdminProductDto dto, CancellationToken cancellationToken = default)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException("المنتج غير موجود.");

            var categoryExists = await _context.Categories.AnyAsync(x => x.Id == dto.CategoryId, cancellationToken);
            if (!categoryExists)
                throw new InvalidOperationException("الكاتيجوري غير موجودة.");

            if (dto.UnitId.HasValue)
            {
                var unitExists = await _context.ProductUnits.AnyAsync(x => x.Id == dto.UnitId.Value && x.IsActive, cancellationToken);
                if (!unitExists)
                    throw new InvalidOperationException("وحدة المنتج غير موجودة أو غير مفعلة.");
            }

            if (string.IsNullOrWhiteSpace(dto.NameAr))
                throw new InvalidOperationException("الاسم العربي مطلوب.");
            if (string.IsNullOrWhiteSpace(dto.NameEn))
                throw new InvalidOperationException("الاسم الإنجليزي مطلوب.");
            if (dto.QuantityStep <= 0)
                throw new InvalidOperationException("QuantityStep يجب أن يكون أكبر من صفر.");
            if (dto.MinPurchaseQuantity <= 0)
                throw new InvalidOperationException("MinPurchaseQuantity يجب أن يكون أكبر من صفر.");

            var imageBytes = ParseBase64Image(dto.ImageBase64);
            var now = DateTime.UtcNow;

            product.CategoryId = dto.CategoryId;
            product.UnitId = dto.UnitId;
            product.NameAr = dto.NameAr.Trim();
            product.NameEn = dto.NameEn.Trim();
            product.DescriptionAr = dto.DescriptionAr?.Trim();
            product.DescriptionEn = dto.DescriptionEn?.Trim();
            if (imageBytes != null)
                product.Image = imageBytes;
            product.Price = dto.Price;
            product.SalePrice = dto.SalePrice;
            product.CostPrice = dto.CostPrice;
            product.StockQuantity = dto.StockQuantity;
            product.MinStockQuantity = dto.MinStockQuantity;
            product.MinPurchaseQuantity = dto.MinPurchaseQuantity;
            product.MaxPurchaseQuantity = dto.MaxPurchaseQuantity;
            product.QuantityStep = dto.QuantityStep;
            product.IsActive = dto.IsActive;
            product.UpdatedAt = now;

            await _context.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(product.Id, cancellationToken);
        }

        private async Task ValidateReferencesAsync(
            CreateAdminProductDto dto,
            CancellationToken cancellationToken)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(x => x.Id == dto.CategoryId, cancellationToken);

            if (!categoryExists)
                throw new InvalidOperationException("الكاتيجوري غير موجودة.");

            if (dto.UnitId.HasValue)
            {
                var unitExists = await _context.ProductUnits
                    .AnyAsync(x => x.Id == dto.UnitId.Value && x.IsActive, cancellationToken);

                if (!unitExists)
                    throw new InvalidOperationException("وحدة المنتج غير موجودة أو غير مفعلة.");
            }

            var variantUnitIds = dto.Variants
                .Where(x => x.UnitId.HasValue)
                .Select(x => x.UnitId!.Value)
                .Distinct()
                .ToList();

            if (variantUnitIds.Count > 0)
            {
                var existingCount = await _context.ProductUnits
                    .CountAsync(x => variantUnitIds.Contains(x.Id) && x.IsActive, cancellationToken);

                if (existingCount != variantUnitIds.Count)
                    throw new InvalidOperationException("إحدى وحدات الـ Variants غير موجودة أو غير مفعلة.");
            }
        }

        private void ValidateProductPayload(CreateAdminProductDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NameAr))
                throw new InvalidOperationException("الاسم العربي مطلوب.");

            if (string.IsNullOrWhiteSpace(dto.NameEn))
                throw new InvalidOperationException("الاسم الإنجليزي مطلوب.");

            if (dto.CategoryId <= 0)
                throw new InvalidOperationException("الكاتيجوري غير صالحة.");

            if (dto.QuantityStep <= 0)
                throw new InvalidOperationException("QuantityStep يجب أن يكون أكبر من صفر.");

            if (dto.MinPurchaseQuantity <= 0)
                throw new InvalidOperationException("MinPurchaseQuantity يجب أن يكون أكبر من صفر.");

            if (dto.MaxPurchaseQuantity.HasValue && dto.MaxPurchaseQuantity.Value < dto.MinPurchaseQuantity)
                throw new InvalidOperationException("MaxPurchaseQuantity يجب أن تكون أكبر أو تساوي MinPurchaseQuantity.");

            if (dto.ProductType == ProductType.Simple)
            {
                if (dto.Options.Count > 0)
                    throw new InvalidOperationException("المنتج البسيط لا يجب أن يحتوي Options.");

                if (dto.Variants.Count > 0)
                    throw new InvalidOperationException("المنتج البسيط لا يجب أن يحتوي Variants.");

                ValidateNumericFields(
                    dto.Price,
                    dto.SalePrice,
                    dto.CostPrice,
                    dto.StockQuantity,
                    dto.MinStockQuantity,
                    dto.MinPurchaseQuantity,
                    dto.MaxPurchaseQuantity,
                    dto.QuantityStep,
                    dto.AllowDecimalQuantity,
                    "المنتج");
            }
            else
            {
                if (dto.Options.Count == 0)
                    throw new InvalidOperationException("المنتج المتغير يجب أن يحتوي Options.");

                if (dto.Variants.Count == 0)
                    throw new InvalidOperationException("المنتج المتغير يجب أن يحتوي Variants.");

                ValidateVariableOptions(dto.Options);
                ValidateVariableVariants(dto);
            }
        }

        private void ValidateVariableOptions(List<CreateAdminProductOptionDto> options)
        {
            var optionNamesAr = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var optionNamesEn = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var valueKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var option in options)
            {
                if (string.IsNullOrWhiteSpace(option.NameAr))
                    throw new InvalidOperationException("اسم الخيار بالعربي مطلوب.");

                if (string.IsNullOrWhiteSpace(option.NameEn))
                    throw new InvalidOperationException("اسم الخيار بالإنجليزي مطلوب.");

                if (!optionNamesAr.Add(option.NameAr.Trim()))
                    throw new InvalidOperationException($"اسم الخيار العربي مكرر: {option.NameAr}");

                if (!optionNamesEn.Add(option.NameEn.Trim()))
                    throw new InvalidOperationException($"اسم الخيار الإنجليزي مكرر: {option.NameEn}");

                if (option.Values.Count == 0)
                    throw new InvalidOperationException($"الخيار {option.NameAr} يجب أن يحتوي قيماً.");

                foreach (var value in option.Values)
                {
                    if (string.IsNullOrWhiteSpace(value.Key))
                        throw new InvalidOperationException("كل قيمة داخل الخيار يجب أن تحتوي Key.");

                    if (string.IsNullOrWhiteSpace(value.ValueAr))
                        throw new InvalidOperationException("القيمة العربية داخل الخيار مطلوبة.");

                    if (string.IsNullOrWhiteSpace(value.ValueEn))
                        throw new InvalidOperationException("القيمة الإنجليزية داخل الخيار مطلوبة.");

                    if (!valueKeys.Add(value.Key.Trim()))
                        throw new InvalidOperationException($"مفتاح القيمة مكرر: {value.Key}");
                }
            }
        }

        private void ValidateVariableVariants(CreateAdminProductDto dto)
        {
            var valueKeyToOptionIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < dto.Options.Count; i++)
            {
                foreach (var value in dto.Options[i].Values)
                {
                    valueKeyToOptionIndex[value.Key.Trim()] = i;
                }
            }

            var defaultCount = dto.Variants.Count(x => x.IsDefault);
            if (defaultCount > 1)
                throw new InvalidOperationException("لا يمكن تحديد أكثر من Default Variant.");

            var uniqueCombinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var variant in dto.Variants)
            {
                ValidateNumericFields(
                    variant.Price,
                    variant.SalePrice,
                    variant.CostPrice,
                    variant.StockQuantity,
                    variant.MinStockQuantity,
                    variant.MinPurchaseQuantity,
                    variant.MaxPurchaseQuantity,
                    variant.QuantityStep,
                    dto.AllowDecimalQuantity,
                    "الـ Variant");

                if (variant.SelectedValueKeys.Count == 0)
                    throw new InvalidOperationException("كل Variant يجب أن يحتوي SelectedValueKeys.");

                var normalizedKeys = variant.SelectedValueKeys
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (normalizedKeys.Count != variant.SelectedValueKeys.Count)
                    throw new InvalidOperationException("يوجد قيم مكررة داخل نفس الـ Variant.");

                if (normalizedKeys.Count != dto.Options.Count)
                    throw new InvalidOperationException("كل Variant يجب أن يختار قيمة واحدة من كل Option.");

                var optionIndexes = new HashSet<int>();

                foreach (var key in normalizedKeys)
                {
                    if (!valueKeyToOptionIndex.TryGetValue(key, out var optionIndex))
                        throw new InvalidOperationException($"الـ Variant يحتوي Key غير معروف: {key}");

                    if (!optionIndexes.Add(optionIndex))
                        throw new InvalidOperationException("لا يمكن اختيار قيمتين من نفس الخيار داخل نفس الـ Variant.");
                }

                var combinationKey = string.Join("|", normalizedKeys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase));

                if (!uniqueCombinations.Add(combinationKey))
                    throw new InvalidOperationException("يوجد Variant مكرر بنفس تركيبة القيم.");
            }
        }

        private void ValidateNumericFields(
            decimal price,
            decimal? salePrice,
            decimal? costPrice,
            decimal stockQuantity,
            decimal minStockQuantity,
            decimal? minPurchaseQuantity,
            decimal? maxPurchaseQuantity,
            decimal? quantityStep,
            bool allowDecimalQuantity,
            string label)
        {
            if (price <= 0)
                throw new InvalidOperationException($"{label}: السعر يجب أن يكون أكبر من صفر.");

            if (salePrice.HasValue && salePrice.Value <= 0)
                throw new InvalidOperationException($"{label}: SalePrice يجب أن تكون أكبر من صفر.");

            if (costPrice.HasValue && costPrice.Value < 0)
                throw new InvalidOperationException($"{label}: CostPrice لا يمكن أن تكون سالبة.");

            if (stockQuantity < 0)
                throw new InvalidOperationException($"{label}: StockQuantity لا يمكن أن تكون سالبة.");

            if (minStockQuantity < 0)
                throw new InvalidOperationException($"{label}: MinStockQuantity لا يمكن أن تكون سالبة.");

            if (minPurchaseQuantity.HasValue && minPurchaseQuantity.Value <= 0)
                throw new InvalidOperationException($"{label}: MinPurchaseQuantity يجب أن تكون أكبر من صفر.");

            if (quantityStep.HasValue && quantityStep.Value <= 0)
                throw new InvalidOperationException($"{label}: QuantityStep يجب أن تكون أكبر من صفر.");

            if (minPurchaseQuantity.HasValue && maxPurchaseQuantity.HasValue &&
                maxPurchaseQuantity.Value < minPurchaseQuantity.Value)
                throw new InvalidOperationException($"{label}: MaxPurchaseQuantity يجب أن تكون أكبر أو تساوي MinPurchaseQuantity.");

            if (!allowDecimalQuantity)
            {
                if (!IsWhole(stockQuantity))
                    throw new InvalidOperationException($"{label}: StockQuantity يجب أن تكون عددًا صحيحًا عند تعطيل الكميات العشرية.");

                if (!IsWhole(minStockQuantity))
                    throw new InvalidOperationException($"{label}: MinStockQuantity يجب أن تكون عددًا صحيحًا عند تعطيل الكميات العشرية.");

                if (minPurchaseQuantity.HasValue && !IsWhole(minPurchaseQuantity.Value))
                    throw new InvalidOperationException($"{label}: MinPurchaseQuantity يجب أن تكون عددًا صحيحًا عند تعطيل الكميات العشرية.");

                if (maxPurchaseQuantity.HasValue && !IsWhole(maxPurchaseQuantity.Value))
                    throw new InvalidOperationException($"{label}: MaxPurchaseQuantity يجب أن تكون عددًا صحيحًا عند تعطيل الكميات العشرية.");

                if (quantityStep.HasValue && !IsWhole(quantityStep.Value))
                    throw new InvalidOperationException($"{label}: QuantityStep يجب أن تكون عددًا صحيحًا عند تعطيل الكميات العشرية.");
            }
        }

        private Product BuildProductEntity(
            CreateAdminProductDto dto,
            byte[]? imageBytes,
            DateTime now)
        {
            if (dto.ProductType == ProductType.Simple)
            {
                return new Product
                {
                    CategoryId = dto.CategoryId,
                    UnitId = dto.UnitId,
                    NameAr = dto.NameAr.Trim(),
                    NameEn = dto.NameEn.Trim(),
                    DescriptionAr = dto.DescriptionAr?.Trim(),
                    DescriptionEn = dto.DescriptionEn?.Trim(),
                    Image = imageBytes,
                    ProductType = dto.ProductType,
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
                    IsActive = true,
                    CreatedAt = now
                };
            }

            var defaultVariant = dto.Variants
                .FirstOrDefault(x => x.IsDefault)
                ?? dto.Variants.OrderBy(x => x.SortOrder).First();

            return new Product
            {
                CategoryId = dto.CategoryId,
                UnitId = dto.UnitId,
                NameAr = dto.NameAr.Trim(),
                NameEn = dto.NameEn.Trim(),
                DescriptionAr = dto.DescriptionAr?.Trim(),
                DescriptionEn = dto.DescriptionEn?.Trim(),
                Image = imageBytes,
                ProductType = dto.ProductType,
                PurchaseInputMode = dto.PurchaseInputMode,
                AllowDecimalQuantity = dto.AllowDecimalQuantity,

                Price = defaultVariant.Price,
                SalePrice = defaultVariant.SalePrice,
                CostPrice = defaultVariant.CostPrice,

                StockQuantity = dto.Variants.Where(x => x.IsActive).Sum(x => x.StockQuantity),
                MinStockQuantity = dto.Variants.Where(x => x.IsActive).Sum(x => x.MinStockQuantity),

                MinPurchaseQuantity = defaultVariant.MinPurchaseQuantity ?? dto.MinPurchaseQuantity,
                MaxPurchaseQuantity = defaultVariant.MaxPurchaseQuantity ?? dto.MaxPurchaseQuantity,
                QuantityStep = defaultVariant.QuantityStep ?? dto.QuantityStep,

                IsActive = true,
                CreatedAt = now
            };
        }

        private void BuildOptions(
            Product product,
            CreateAdminProductDto dto,
            DateTime now,
            Dictionary<string, ProductOptionValue> optionValueMap)
        {
            foreach (var optionDto in dto.Options)
            {
                var option = new ProductOption
                {
                    Product = product,
                    NameAr = optionDto.NameAr.Trim(),
                    NameEn = optionDto.NameEn.Trim(),
                    SortOrder = optionDto.SortOrder,
                    CreatedAt = now
                };

                _context.ProductOptions.Add(option);

                foreach (var valueDto in optionDto.Values.OrderBy(x => x.SortOrder))
                {
                    var key = valueDto.Key.Trim();

                    var value = new ProductOptionValue
                    {
                        ProductOption = option,
                        ValueAr = valueDto.ValueAr.Trim(),
                        ValueEn = valueDto.ValueEn.Trim(),
                        ColorHex = string.IsNullOrWhiteSpace(valueDto.ColorHex) ? null : valueDto.ColorHex.Trim(),
                        SortOrder = valueDto.SortOrder,
                        CreatedAt = now
                    };

                    _context.ProductOptionValues.Add(value);
                    optionValueMap[key] = value;
                }
            }
        }

        private void BuildVariants(
            Product product,
            CreateAdminProductDto dto,
            DateTime now,
            Dictionary<string, ProductOptionValue> optionValueMap)
        {
            var hasDefault = dto.Variants.Any(x => x.IsDefault);
            var orderedVariants = dto.Variants.OrderBy(x => x.SortOrder).ToList();

            for (var i = 0; i < orderedVariants.Count; i++)
            {
                var variantDto = orderedVariants[i];

                var variant = new ProductVariant
                {
                    Product = product,
                    UnitId = variantDto.UnitId,
                    NameAr = variantDto.NameAr?.Trim(),
                    NameEn = variantDto.NameEn?.Trim(),
                    SKU = variantDto.SKU?.Trim(),
                    Barcode = variantDto.Barcode?.Trim(),
                    Price = variantDto.Price,
                    SalePrice = variantDto.SalePrice,
                    CostPrice = variantDto.CostPrice,
                    StockQuantity = variantDto.StockQuantity,
                    MinStockQuantity = variantDto.MinStockQuantity,
                    MinPurchaseQuantity = variantDto.MinPurchaseQuantity,
                    MaxPurchaseQuantity = variantDto.MaxPurchaseQuantity,
                    QuantityStep = variantDto.QuantityStep,
                    IsDefault = hasDefault ? variantDto.IsDefault : i == 0,
                    IsActive = variantDto.IsActive,
                    SortOrder = variantDto.SortOrder,
                    CreatedAt = now
                };

                _context.ProductVariants.Add(variant);

                foreach (var selectedKey in variantDto.SelectedValueKeys
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var value = optionValueMap[selectedKey];

                    _context.ProductVariantOptionValues.Add(new ProductVariantOptionValue
                    {
                        ProductVariant = variant,
                        ProductOptionValue = value
                    });
                }
            }
        }

        private byte[]? ParseBase64Image(string? base64)
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

        private bool IsWhole(decimal value)
        {
            return value == decimal.Truncate(value);
        }
    }
}