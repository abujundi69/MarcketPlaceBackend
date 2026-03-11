using MarcketPlace.Application.Customer.Stores.Dtos;

namespace MarcketPlace.Application.Customer.Stores
{
    public interface ICustomerStoreCatalogService
    {
        Task<IReadOnlyList<CustomerStoreProductDto>> GetStoreProductsAsync(
            int storeId,
            int? categoryId,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StoreListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<StoreDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}