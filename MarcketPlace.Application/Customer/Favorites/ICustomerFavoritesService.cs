using MarcketPlace.Application.Customer.Favorites.Dtos;

namespace MarcketPlace.Application.Customer.Favorites
{
    public interface ICustomerFavoritesService
    {
        Task<IReadOnlyList<CustomerFavoriteProductDto>> GetFavoritesAsync(
            int customerUserId,
            CancellationToken cancellationToken = default);

        Task AddFavoriteAsync(
            int customerUserId,
            int productId,
            CancellationToken cancellationToken = default);

        Task RemoveFavoriteAsync(
            int customerUserId,
            int productId,
            CancellationToken cancellationToken = default);
    }
}
