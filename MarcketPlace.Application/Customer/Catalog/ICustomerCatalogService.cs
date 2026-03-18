using MarcketPlace.Application.Customer.Catalog.Dtos;

namespace MarcketPlace.Application.Customer.Catalog
{
    public interface ICustomerCatalogService
    {
        Task<IReadOnlyList<CustomerCategoryDto>> GetCategoriesAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CustomerProductListItemDto>> GetProductsByCategoryAsync(
            int categoryId,
            CancellationToken cancellationToken = default);

        Task<CustomerProductDetailsDto> GetProductDetailsAsync(
            int productId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CustomerMostOrderedProductDto>> GetMostOrderedProductsAsync(
            int take = 10,
            CancellationToken cancellationToken = default);
    }
}