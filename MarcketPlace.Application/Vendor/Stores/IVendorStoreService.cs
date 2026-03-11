using MarcketPlace.Application.Vendor.Stores.Dtos;

namespace MarcketPlace.Application.Vendor.Stores
{
    public interface IVendorStoreService
    {
        Task<IReadOnlyList<VendorStoreListItemDto>> GetMyStoresAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<VendorStoreWorkingHoursDto>> GetMyStoresWorkingHoursAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<VendorStoreWorkingHoursDto> GetStoreWorkingHoursAsync(
            int userId,
            int storeId,
            CancellationToken cancellationToken = default);

        Task<VendorStoreWorkingHoursDto> UpdateStoreWorkingHoursAsync(
            int userId,
            int storeId,
            UpdateVendorStoreWorkingHoursDto dto,
            CancellationToken cancellationToken = default);
    }
}