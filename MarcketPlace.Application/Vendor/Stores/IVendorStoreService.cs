using MarcketPlace.Application.Vendor.Stores.Dtos;

namespace MarcketPlace.Application.Vendor.Stores
{
    public interface IVendorStoreService
    {
        Task<IReadOnlyList<VendorStoreListItemDto>> GetMyStoresAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}
