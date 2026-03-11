using MarcketPlace.Application.Customer.Cart.Dtos;

namespace MarcketPlace.Application.Customer.Cart
{
    public interface ICustomerCartService
    {
        Task<CustomerCartDto> GetCartAsync(
            int customerUserId,
            CancellationToken cancellationToken = default);

        Task<CustomerCartDto> AddItemAsync(
            int customerUserId,
            AddCustomerCartItemDto dto,
            CancellationToken cancellationToken = default);

        Task<CustomerCartDto> UpdateQuantityAsync(
            int customerUserId,
            int cartItemId,
            UpdateCustomerCartItemQuantityDto dto,
            CancellationToken cancellationToken = default);

        Task RemoveItemAsync(
            int customerUserId,
            int cartItemId,
            CancellationToken cancellationToken = default);

        Task ClearCartAsync(
            int customerUserId,
            CancellationToken cancellationToken = default);
    }
}