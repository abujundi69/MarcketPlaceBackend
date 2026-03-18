using MarcketPlace.Application.Shared.Orders.Dtos;

namespace MarcketPlace.Application.Driver.Orders
{
    public interface IDriverOrderService
    {
        Task<IReadOnlyList<OrderListItemDto>> GetAvailableOrdersAsync(
            int driverUserId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<OrderListItemDto>> GetMyOrdersAsync(
            int driverUserId,
            CancellationToken cancellationToken = default);

        Task<OrderDetailsDto> GetByIdAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default);

        Task<OrderDetailsDto> AcceptOrderAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default);

        Task<OrderDetailsDto> MarkPickedUpAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default);

        Task<OrderDetailsDto> MarkDeliveredAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default);

        Task CancelAssignmentAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default);
    }
}