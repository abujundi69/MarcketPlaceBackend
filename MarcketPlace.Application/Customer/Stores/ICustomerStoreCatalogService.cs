using MarcketPlace.Application.Customer.Stores.Dtos;

namespace MarcketPlace.Application.Customer.Stores
{
    public interface ICustomerStoreCatalogService
    {
        Task<IReadOnlyList<CustomerStoreProductDto>> GetStoreProductsAsync(
            int storeId,
            int? categoryId,
            CancellationToken cancellationToken = default);
    }
}