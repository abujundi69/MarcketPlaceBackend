using MarcketPlace.Application.Admin.Products.Dtos;
using MarcketPlace.Application.Admin.ProductsDoscount.Dtos;

namespace MarcketPlace.Application.Admin.Products
{
    public interface IAdminProductDiscountService
    {
        Task SetProductDiscountAsync(
            int productId,
            SetProductDiscountDto dto,
            CancellationToken cancellationToken = default);

        Task ClearProductDiscountAsync(
            int productId,
            CancellationToken cancellationToken = default);

        Task SetVariantDiscountAsync(
            int variantId,
            SetVariantDiscountDto dto,
            CancellationToken cancellationToken = default);

        Task ClearVariantDiscountAsync(
            int variantId,
            CancellationToken cancellationToken = default);
    }
}