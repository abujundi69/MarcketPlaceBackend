using MarcketPlace.Application.Admin.Products.Dtos;
using MarcketPlace.Application.Admin.ProductsDoscount.Dtos;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Admin.Products
{
    public class AdminProductDiscountService : IAdminProductDiscountService
    {
        private readonly AppDbContext _context;

        public AdminProductDiscountService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SetProductDiscountAsync(
            int productId,
            SetProductDiscountDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);

            if (product is null)
                throw new KeyNotFoundException("المنتج غير موجود.");

            product.SalePrice = CalculateDiscountedPrice(product.Price, dto.DiscountPercentage, dto.DiscountedPrice);
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task ClearProductDiscountAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == productId, cancellationToken);

            if (product is null)
                throw new KeyNotFoundException("المنتج غير موجود.");

            product.SalePrice = null;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task SetVariantDiscountAsync(
            int variantId,
            SetVariantDiscountDto dto,
            CancellationToken cancellationToken = default)
        {
            if (dto is null)
                throw new InvalidOperationException("البيانات المرسلة غير صالحة.");

            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(x => x.Id == variantId, cancellationToken);

            if (variant is null)
                throw new KeyNotFoundException("الـ Variant غير موجود.");

            variant.SalePrice = CalculateDiscountedPrice(variant.Price, dto.DiscountPercentage, dto.DiscountedPrice);
            variant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task ClearVariantDiscountAsync(
            int variantId,
            CancellationToken cancellationToken = default)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(x => x.Id == variantId, cancellationToken);

            if (variant is null)
                throw new KeyNotFoundException("الـ Variant غير موجود.");

            variant.SalePrice = null;
            variant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        private decimal CalculateDiscountedPrice(
            decimal originalPrice,
            decimal? discountPercentage,
            decimal? discountedPrice)
        {
            if (discountPercentage.HasValue && discountedPrice.HasValue)
                throw new InvalidOperationException("اختر إما نسبة خصم أو سعر بعد الخصم فقط.");

            if (!discountPercentage.HasValue && !discountedPrice.HasValue)
                throw new InvalidOperationException("يجب إرسال نسبة خصم أو سعر بعد الخصم.");

            decimal result;

            if (discountPercentage.HasValue)
            {
                if (discountPercentage.Value <= 0 || discountPercentage.Value >= 100)
                    throw new InvalidOperationException("نسبة الخصم يجب أن تكون أكبر من 0 وأقل من 100.");

                result = originalPrice - (originalPrice * discountPercentage.Value / 100m);
            }
            else
            {
                result = discountedPrice!.Value;

                if (result <= 0 || result >= originalPrice)
                    throw new InvalidOperationException("السعر بعد الخصم يجب أن يكون أكبر من 0 وأقل من السعر الأصلي.");
            }

            return decimal.Round(result, 2, MidpointRounding.AwayFromZero);
        }
    }
}