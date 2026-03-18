using MarcketPlace.Application.Customer.Favorites.Dtos;

namespace MarcketPlace.Application.Customer.Favorites
{
    public interface ICustomerFavoriteService
    {
        Task<IReadOnlyList<CustomerFavoriteProductDto>> GetMyFavoritesAsync(
            int customerUserId,
            CancellationToken cancellationToken = default);

        Task<CustomerFavoriteToggleResultDto> AddAsync(
            int customerUserId,
            int productId,
            CancellationToken cancellationToken = default);

        Task<CustomerFavoriteToggleResultDto> RemoveAsync(
            int customerUserId,
            int productId,
            CancellationToken cancellationToken = default);

        Task<CustomerFavoriteToggleResultDto> ToggleAsync(
            int customerUserId,
            int productId,
            CancellationToken cancellationToken = default);
    }
}