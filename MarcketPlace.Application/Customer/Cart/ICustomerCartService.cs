using MarcketPlace.Application.Customer.Cart.Dtos;

namespace MarcketPlace.Application.Customer.Cart
{
    public interface ICustomerCartService
    {
        Task<CustomerCartDto> GetCartAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<CustomerCartDto> AddItemAsync(
            int userId,
            AddCustomerCartItemDto dto,
            CancellationToken cancellationToken = default);

        Task<CustomerCartDto> UpdateItemAsync(
            int userId,
            int cartItemId,
            UpdateCustomerCartItemDto dto,
            CancellationToken cancellationToken = default);

        Task<CustomerCartDto> RemoveItemAsync(
            int userId,
            int cartItemId,
            CancellationToken cancellationToken = default);

        Task<CustomerCartDto> ClearAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<CustomerCartDto> ReorderLastOrderAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}