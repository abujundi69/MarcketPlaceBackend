using MarcketPlace.Application.Customer.Orders.Dtos;

namespace MarcketPlace.Application.Customer.Orders
{
    public interface ICustomerOrderService
    {
        Task<CustomerCreatedOrderDto> CreateFromCartAsync(
            int customerUserId,
            CreateCustomerOrderFromCartDto dto,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CustomerOrderListItemDto>> GetMyOrdersAsync(
            int customerUserId,
            CancellationToken cancellationToken = default);

        Task<CustomerOrderDetailsDto> GetByIdAsync(
            int customerUserId,
            int orderId,
            CancellationToken cancellationToken = default);
    }
}