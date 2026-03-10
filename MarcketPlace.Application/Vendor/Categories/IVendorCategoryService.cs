using MarcketPlace.Application.Vendor.Categories.Dtos;

namespace MarcketPlace.Application.Vendor.Categories
{
    public interface IVendorCategoryService
    {
        Task<IReadOnlyList<VendorCategoryListItemDto>> GetActiveCategoriesAsync(
            CancellationToken cancellationToken = default);
    }
}
