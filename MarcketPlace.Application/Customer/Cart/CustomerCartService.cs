using MarcketPlace.Application.Customer.Cart.Dtos;
using MarcketPlace.Domain.Entities;
using MarcketPlace.Domain.Enums;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Customer.Cart
{
    public class CustomerCartService : ICustomerCartService
    {
        private readonly AppDbContext _context;

        public CustomerCartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CustomerCartDto> GetCartAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(userId, cancellationToken);
            return await BuildCartAsync(customerId, cancellationToken);
        }

        public async Task<CustomerCartDto> AddItemAsync(
            int userId,
            AddCustomerCartItemDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            var customerId = await GetCustomerIdAsync(userId, cancellationToken);

            var resolved = await ResolveCartLineAsync(
                dto.ProductId,
                dto.ProductVariantId,
                dto.SelectedOptionValueIds,
                dto.PurchaseEntryMode,
                dto.Quantity,
                dto.RequestedAmount,
                cancellationToken);

            await UpsertResolvedCartLineAsync(customerId, resolved, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return await BuildCartAsync(customerId, cancellationToken);
        }

        public async Task<CustomerCartDto> UpdateItemAsync(
            int userId,
            int cartItemId,
            UpdateCustomerCartItemDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            var customerId = await GetCustomerIdAsync(userId, cancellationToken);

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(x => x.Id == cartItemId && x.CustomerId == customerId, cancellationToken);

            if (cartItem is null)
                throw new KeyNotFoundException("عنصر السلة غير موجود.");

            var resolved = await ResolveCartLineAsync(
                cartItem.ProductId,
                dto.ProductVariantId ?? cartItem.ProductVariantId,
                dto.SelectedOptionValueIds,
                dto.PurchaseEntryMode,
                dto.Quantity,
                dto.RequestedAmount,
                cancellationToken);

            var resolvedVariantId = resolved.ProductVariant?.Id;

            var duplicateItem = await _context.CartItems
                .FirstOrDefaultAsync(x =>
                    x.Id != cartItem.Id &&
                    x.CustomerId == customerId &&
                    x.ProductId == resolved.Product.Id &&
                    x.ProductVariantId == resolvedVariantId &&
                    x.PurchaseEntryMode == resolved.PurchaseEntryMode &&
                    x.UnitPrice == resolved.UnitPrice,
                    cancellationToken);

            if (duplicateItem is null)
            {
                cartItem.ProductVariantId = resolvedVariantId;
                cartItem.Quantity = resolved.Quantity;
                cartItem.RequestedAmount = resolved.RequestedAmount;
                cartItem.PurchaseEntryMode = resolved.PurchaseEntryMode;
                cartItem.UnitPrice = resolved.UnitPrice;
                cartItem.LineTotal = resolved.LineTotal;
                cartItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var mergedQuantity = duplicateItem.Quantity + resolved.Quantity;
                var mergedRequestedAmount = resolved.PurchaseEntryMode == ProductPurchaseInputMode.AmountOnly
                    ? (duplicateItem.RequestedAmount ?? 0m) + (resolved.RequestedAmount ?? 0m)
                    : (decimal?)null;

                ValidateStock(mergedQuantity, resolved.StockQuantity);

                duplicateItem.Quantity = mergedQuantity;
                duplicateItem.RequestedAmount = mergedRequestedAmount;
                duplicateItem.UnitPrice = resolved.UnitPrice;
                duplicateItem.LineTotal = resolved.PurchaseEntryMode == ProductPurchaseInputMode.AmountOnly
                    ? RoundMoney(mergedRequestedAmount ?? 0m)
                    : RoundMoney(mergedQuantity * resolved.UnitPrice);
                duplicateItem.UpdatedAt = DateTime.UtcNow;

                _context.CartItems.Remove(cartItem);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return await BuildCartAsync(customerId, cancellationToken);
        }

        public async Task<CustomerCartDto> RemoveItemAsync(
            int userId,
            int cartItemId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(userId, cancellationToken);

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(x => x.Id == cartItemId && x.CustomerId == customerId, cancellationToken);

            if (cartItem is null)
                throw new KeyNotFoundException("عنصر السلة غير موجود.");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync(cancellationToken);

            return await BuildCartAsync(customerId, cancellationToken);
        }

        public async Task<CustomerCartDto> ClearAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(userId, cancellationToken);

            var items = await _context.CartItems
                .Where(x => x.CustomerId == customerId)
                .ToListAsync(cancellationToken);

            if (items.Count > 0)
            {
                _context.CartItems.RemoveRange(items);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return await BuildCartAsync(customerId, cancellationToken);
        }

        public async Task<CustomerCartDto> ReorderLastOrderAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var customerId = await GetCustomerIdAsync(userId, cancellationToken);

            var lastOrder = await _context.Orders
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x =>
                    x.CustomerId == customerId &&
                    x.CancelledAt == null &&
                    x.Status != OrderStatus.Cancelled)
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastOrder is null)
                throw new KeyNotFoundException("لا يوجد طلب سابق لإعادة طلبه.");

            if (lastOrder.Items.Count == 0)
                throw new InvalidOperationException("آخر طلب لا يحتوي عناصر.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var currentCartItems = await _context.CartItems
                .Where(x => x.CustomerId == customerId)
                .ToListAsync(cancellationToken);

            if (currentCartItems.Count > 0)
                _context.CartItems.RemoveRange(currentCartItems);

            foreach (var item in lastOrder.Items.OrderBy(x => x.Id))
            {
                var resolved = await ResolveCartLineAsync(
                    item.ProductId,
                    item.ProductVariantId,
                    null,
                    item.PurchaseInputMode,
                    item.PurchaseInputMode == ProductPurchaseInputMode.QuantityOnly ? item.Quantity : null,
                    item.PurchaseInputMode == ProductPurchaseInputMode.AmountOnly ? item.RequestedAmount : null,
                    cancellationToken);

                await UpsertResolvedCartLineAsync(customerId, resolved, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return await BuildCartAsync(customerId, cancellationToken);
        }

        private async Task<int> GetCustomerIdAsync(
            int userId,
            CancellationToken cancellationToken)
        {
            var customerId = await _context.Customers
                .Where(x => x.UserId == userId)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (!customerId.HasValue)
                throw new KeyNotFoundException("العميل غير موجود.");

            return customerId.Value;
        }

        private async Task<CustomerCartDto> BuildCartAsync(
            int customerId,
            CancellationToken cancellationToken)
        {
            var cartItems = await _context.CartItems
                .AsNoTracking()
                .Include(x => x.Product)
                    .ThenInclude(x => x.Unit)
                .Include(x => x.ProductVariant)
                    .ThenInclude(x => x.Unit)
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            var items = cartItems.Select(x =>
            {
                var product = x.Product ?? throw new InvalidOperationException("بيانات المنتج المرتبطة بالسلة غير موجودة.");
                var variant = x.ProductVariant;

                string? unitSymbol = variant is not null
                    ? variant.Unit?.Symbol ?? product.Unit?.Symbol
                    : product.Unit?.Symbol;

                return new CustomerCartItemDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductVariantId = x.ProductVariantId,
                    ProductNameAr = product.NameAr,
                    ProductNameEn = product.NameEn,
                    ProductImage = product.Image,
                    VariantNameAr = variant?.NameAr,
                    VariantNameEn = variant?.NameEn,
                    UnitSymbol = unitSymbol,
                    PurchaseEntryMode = x.PurchaseEntryMode,
                    UnitPrice = x.UnitPrice,
                    Quantity = x.Quantity,
                    RequestedAmount = x.RequestedAmount,
                    LineTotal = x.LineTotal,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt
                };
            }).ToList();

            return new CustomerCartDto
            {
                ItemsCount = items.Count,
                Subtotal = RoundMoney(items.Sum(x => x.LineTotal)),
                Items = items
            };
        }

        private async Task UpsertResolvedCartLineAsync(
            int customerId,
            ResolvedCartLine resolved,
            CancellationToken cancellationToken)
        {
            var resolvedVariantId = resolved.ProductVariant?.Id;

            var existing = await _context.CartItems
                .FirstOrDefaultAsync(x =>
                    x.CustomerId == customerId &&
                    x.ProductId == resolved.Product.Id &&
                    x.ProductVariantId == resolvedVariantId &&
                    x.PurchaseEntryMode == resolved.PurchaseEntryMode &&
                    x.UnitPrice == resolved.UnitPrice,
                    cancellationToken);

            if (existing is null)
            {
                _context.CartItems.Add(new CartItem
                {
                    CustomerId = customerId,
                    ProductId = resolved.Product.Id,
                    ProductVariantId = resolvedVariantId,
                    Quantity = resolved.Quantity,
                    RequestedAmount = resolved.RequestedAmount,
                    PurchaseEntryMode = resolved.PurchaseEntryMode,
                    UnitPrice = resolved.UnitPrice,
                    LineTotal = resolved.LineTotal,
                    CreatedAt = DateTime.UtcNow
                });

                return;
            }

            var mergedQuantity = existing.Quantity + resolved.Quantity;
            var mergedRequestedAmount = resolved.PurchaseEntryMode == ProductPurchaseInputMode.AmountOnly
                ? (existing.RequestedAmount ?? 0m) + (resolved.RequestedAmount ?? 0m)
                : (decimal?)null;

            ValidateStock(mergedQuantity, resolved.StockQuantity);

            existing.Quantity = mergedQuantity;
            existing.RequestedAmount = mergedRequestedAmount;
            existing.UnitPrice = resolved.UnitPrice;
            existing.LineTotal = resolved.PurchaseEntryMode == ProductPurchaseInputMode.AmountOnly
                ? RoundMoney(mergedRequestedAmount ?? 0m)
                : RoundMoney(mergedQuantity * resolved.UnitPrice);
            existing.UpdatedAt = DateTime.UtcNow;
        }

        private async Task<ResolvedCartLine> ResolveCartLineAsync(
            int productId,
            int? productVariantId,
            IEnumerable<int>? selectedOptionValueIds,
            ProductPurchaseInputMode purchaseEntryMode,
            decimal? quantityInput,
            decimal? requestedAmountInput,
            CancellationToken cancellationToken)
        {
            if (purchaseEntryMode == ProductPurchaseInputMode.QuantityOrAmount)
                throw new InvalidOperationException("في السلة يجب تحديد طريقة إدخال واحدة فقط: كمية أو مبلغ.");

            var product = await _context.Products
                .Include(x => x.Unit)
                .Include(x => x.Category)
                .Include(x => x.Options)
                    .ThenInclude(o => o.Values)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.Unit)
                .Include(x => x.Variants)
                    .ThenInclude(v => v.VariantOptionValues)
                .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);

            if (product is null)
                throw new KeyNotFoundException("المنتج غير موجود.");

            var category = product.Category;
            if (category is null || !category.IsActive)
                throw new KeyNotFoundException("المنتج غير موجود.");

            if (!product.IsActive)
                throw new KeyNotFoundException("المنتج غير موجود.");

            var variant = ResolveVariant(product, productVariantId, selectedOptionValueIds);

            var effectivePurchaseMode = GetEffectivePurchaseMode(product, variant);
            var effectiveAllowDecimalQuantity = GetEffectiveAllowDecimalQuantity(product, variant);

            ValidateEntryMode(effectivePurchaseMode, purchaseEntryMode, effectiveAllowDecimalQuantity);

            var unitPrice = variant != null
                ? (variant.SalePrice ?? variant.Price)
                : (product.SalePrice ?? product.Price);

            if (unitPrice <= 0)
                throw new InvalidOperationException("سعر المنتج غير صالح.");

            var minPurchaseQuantity = variant?.MinPurchaseQuantity ?? product.MinPurchaseQuantity;
            var maxPurchaseQuantity = variant?.MaxPurchaseQuantity ?? product.MaxPurchaseQuantity;
            var quantityStep = variant?.QuantityStep ?? product.QuantityStep;
            var stockQuantity = variant?.StockQuantity ?? product.StockQuantity;

            decimal quantity;
            decimal? requestedAmount;
            decimal lineTotal;

            if (purchaseEntryMode == ProductPurchaseInputMode.QuantityOnly)
            {
                if (!quantityInput.HasValue || quantityInput.Value <= 0)
                    throw new InvalidOperationException("الكمية المطلوبة غير صالحة.");

                if (requestedAmountInput.HasValue)
                    throw new InvalidOperationException("لا ترسل RequestedAmount عند الإدخال بالكمية.");

                quantity = NormalizeQuantity(quantityInput.Value, effectiveAllowDecimalQuantity);
                requestedAmount = null;

                ValidateQuantity(
                    quantity,
                    effectiveAllowDecimalQuantity,
                    minPurchaseQuantity,
                    maxPurchaseQuantity,
                    quantityStep,
                    validateStep: true);

                lineTotal = RoundMoney(quantity * unitPrice);
            }
            else
            {
                if (!requestedAmountInput.HasValue || requestedAmountInput.Value <= 0)
                    throw new InvalidOperationException("المبلغ المطلوب غير صالح.");

                if (quantityInput.HasValue)
                    throw new InvalidOperationException("لا ترسل Quantity عند الإدخال بالمبلغ.");

                requestedAmount = RoundMoney(requestedAmountInput.Value);
                quantity = NormalizeQuantity(requestedAmount.Value / unitPrice, effectiveAllowDecimalQuantity);

                if (quantity <= 0)
                    throw new InvalidOperationException("المبلغ المدخل ينتج كمية غير صالحة.");

                ValidateQuantity(
                    quantity,
                    effectiveAllowDecimalQuantity,
                    minPurchaseQuantity,
                    maxPurchaseQuantity,
                    quantityStep,
                    validateStep: false);

                lineTotal = requestedAmount.Value;
            }

            ValidateStock(quantity, stockQuantity);

            return new ResolvedCartLine
            {
                Product = product,
                ProductVariant = variant,
                PurchaseEntryMode = purchaseEntryMode,
                UnitPrice = unitPrice,
                Quantity = quantity,
                RequestedAmount = requestedAmount,
                LineTotal = lineTotal,
                StockQuantity = stockQuantity
            };
        }

        private ProductVariant? ResolveVariant(
            Product product,
            int? productVariantId,
            IEnumerable<int>? selectedOptionValueIds)
        {
            var selectedIds = (selectedOptionValueIds ?? Enumerable.Empty<int>())
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            if (product.ProductType != ProductType.Variable)
            {
                if (productVariantId.HasValue)
                    throw new InvalidOperationException("المنتج العادي لا يقبل ProductVariantId.");

                if (selectedIds.Count > 0)
                    throw new InvalidOperationException("المنتج العادي لا يقبل SelectedOptionValueIds.");

                return null;
            }

            var activeVariants = product.Variants
                .Where(v => v.IsActive)
                .ToList();

            if (activeVariants.Count == 0)
                throw new InvalidOperationException("لا توجد Variants مفعّلة لهذا المنتج.");

            if (productVariantId.HasValue)
            {
                var variant = activeVariants.FirstOrDefault(v => v.Id == productVariantId.Value);

                if (variant is null)
                    throw new InvalidOperationException("الـ Variant المحدد غير موجود أو غير مفعّل.");

                if (selectedIds.Count > 0)
                {
                    var variantValueIds = GetVariantOptionValueIds(variant);

                    if (!variantValueIds.SequenceEqual(selectedIds))
                        throw new InvalidOperationException("الـ Variant المحدد لا يطابق الخيارات المختارة.");
                }

                return variant;
            }

            if (selectedIds.Count > 0)
            {
                var matchedVariants = activeVariants
                    .Where(v => GetVariantOptionValueIds(v).SequenceEqual(selectedIds))
                    .ToList();

                if (matchedVariants.Count == 0)
                    throw new InvalidOperationException("لا يوجد Variant يطابق الخيارات المختارة.");

                if (matchedVariants.Count > 1)
                    throw new InvalidOperationException("يوجد أكثر من Variant بنفس الخيارات المختارة، راجع البيانات.");

                return matchedVariants[0];
            }

            if (activeVariants.Count == 1)
                return activeVariants[0];

            throw new InvalidOperationException("هذا المنتج يحتوي Variants ويجب اختيار Variant أو تحديد القيم المختارة.");
        }

        private List<int> GetVariantOptionValueIds(ProductVariant variant)
        {
            return variant.VariantOptionValues
                .Select(x => x.ProductOptionValueId)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }

        private ProductPurchaseInputMode GetEffectivePurchaseMode(Product product, ProductVariant? variant)
        {
            return variant?.PurchaseInputModeOverride ?? product.PurchaseInputMode;
        }

        private bool GetEffectiveAllowDecimalQuantity(Product product, ProductVariant? variant)
        {
            return variant?.AllowDecimalQuantityOverride ?? product.AllowDecimalQuantity;
        }

        private void ValidateEntryMode(
            ProductPurchaseInputMode productMode,
            ProductPurchaseInputMode requestedMode,
            bool allowDecimalQuantity)
        {
            switch (productMode)
            {
                case ProductPurchaseInputMode.QuantityOnly:
                    if (requestedMode != ProductPurchaseInputMode.QuantityOnly)
                        throw new InvalidOperationException("هذا المنتج يقبل الطلب بالكمية فقط.");
                    break;

                case ProductPurchaseInputMode.AmountOnly:
                    if (requestedMode != ProductPurchaseInputMode.AmountOnly)
                        throw new InvalidOperationException("هذا المنتج يقبل الطلب بالمبلغ فقط.");

                    if (!allowDecimalQuantity)
                        throw new InvalidOperationException("هذا المنتج لا يناسب الطلب بالمبلغ.");
                    break;

                case ProductPurchaseInputMode.QuantityOrAmount:
                    if (requestedMode != ProductPurchaseInputMode.QuantityOnly &&
                        requestedMode != ProductPurchaseInputMode.AmountOnly)
                    {
                        throw new InvalidOperationException("طريقة الإدخال غير صالحة.");
                    }

                    if (requestedMode == ProductPurchaseInputMode.AmountOnly && !allowDecimalQuantity)
                        throw new InvalidOperationException("هذا المنتج لا يناسب الطلب بالمبلغ.");
                    break;

                default:
                    throw new InvalidOperationException("طريقة إدخال المنتج غير مدعومة.");
            }
        }

        private void ValidateQuantity(
            decimal quantity,
            bool allowDecimalQuantity,
            decimal minPurchaseQuantity,
            decimal? maxPurchaseQuantity,
            decimal quantityStep,
            bool validateStep)
        {
            if (quantity <= 0)
                throw new InvalidOperationException("الكمية غير صالحة.");

            if (quantity < minPurchaseQuantity)
                throw new InvalidOperationException("الكمية أقل من الحد الأدنى المسموح.");

            if (maxPurchaseQuantity.HasValue && quantity > maxPurchaseQuantity.Value)
                throw new InvalidOperationException("الكمية أكبر من الحد الأعلى المسموح.");

            if (!allowDecimalQuantity && quantity != decimal.Truncate(quantity))
                throw new InvalidOperationException("هذا المنتج لا يقبل كميات عشرية.");

            if (validateStep && quantityStep > 0)
            {
                if (!IsMultipleOf(quantity, quantityStep))
                    throw new InvalidOperationException("الكمية لا تطابق خطوة الزيادة المحددة للمنتج.");
            }
        }

        private void ValidateStock(decimal quantity, decimal stockQuantity)
        {
            if (quantity > stockQuantity)
                throw new InvalidOperationException("الكمية المطلوبة غير متوفرة في المخزون.");
        }

        private decimal NormalizeQuantity(decimal value, bool allowDecimalQuantity)
        {
            return allowDecimalQuantity
                ? decimal.Round(value, 4, MidpointRounding.AwayFromZero)
                : decimal.Round(value, 0, MidpointRounding.AwayFromZero);
        }

        private decimal RoundMoney(decimal value)
        {
            return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private bool IsMultipleOf(decimal value, decimal step)
        {
            if (step <= 0)
                return true;

            return value % step == 0;
        }

        private sealed class ResolvedCartLine
        {
            public Product Product { get; set; } = default!;
            public ProductVariant? ProductVariant { get; set; }
            public ProductPurchaseInputMode PurchaseEntryMode { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Quantity { get; set; }
            public decimal? RequestedAmount { get; set; }
            public decimal LineTotal { get; set; }
            public decimal StockQuantity { get; set; }
        }
    }
}