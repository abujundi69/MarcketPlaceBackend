using MarcketPlace.Application.Admin.Stores.Dtos;

namespace MarcketPlace.Application.Admin.Stores
{
    public interface IStoreAdminService
    {
        Task<StoreAdminDetailsDto> CreateAsync(CreateStoreByAdminDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StoreAdminListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<StoreAdminDetailsDto?> GetByIdAsync(int storeId, CancellationToken cancellationToken = default);
        Task<StoreAdminDetailsDto> UpdateAsync(int storeId, UpdateStoreByAdminDto dto, CancellationToken cancellationToken = default);
    }
}