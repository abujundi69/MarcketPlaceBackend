using MarcketPlace.Application.Customer.Orders.Dtos;
using MarcketPlace.Application.Shared.Orders.Dtos;

namespace MarcketPlace.Application.Customer.Orders
{
    public interface ICustomerOrderService
    {
        Task<CustomerCreatedOrderDto> CreateFromCartAsync(
            int customerUserId,
            CreateCustomerOrderFromCartDto dto,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<OrderListItemDto>> GetMyOrdersAsync(
            int customerUserId,
            CancellationToken cancellationToken = default);

        Task<OrderDetailsDto> GetByIdAsync(
            int customerUserId,
            int orderId,
            CancellationToken cancellationToken = default);

        Task<OrderDetailsDto> CancelAsync(
            int customerUserId,
            int orderId,
            CancelCustomerOrderDto dto,
            CancellationToken cancellationToken = default);
    }
}