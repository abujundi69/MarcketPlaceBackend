using MarcketPlace.Application.Customer.Stores.Dtos;

namespace MarcketPlace.Application.Customer.Stores
{
    public interface ICustomerStoreCatalogService
    {
        Task<IReadOnlyList<StoreCategoryDto>> GetStoreCategoriesAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CustomerStoreProductDto>> GetStoreProductsAsync(
            int storeId,
            int? categoryId,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<StoreListItemDto>> GetAllAsync(int? categoryId = null, CancellationToken cancellationToken = default);
        Task<StoreDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}