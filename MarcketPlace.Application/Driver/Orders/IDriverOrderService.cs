using MarcketPlace.Application.Driver.Orders.Dtos;

namespace MarcketPlace.Application.Driver.Orders
{
    public interface IDriverOrderService
    {
        Task<IReadOnlyList<DriverOrderDto>> GetAvailableOrdersAsync(
            int driverUserId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<DriverOrderDto>> GetMyOrdersAsync(
            int driverUserId,
            CancellationToken cancellationToken = default);

        Task<DriverOrderDto> AcceptOrderAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default);

        Task<DriverOrderDto> PickUpOrderAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default);

        Task<DriverOrderDto> DeliverOrderAsync(
            int driverUserId,
            int orderId,
            CancellationToken cancellationToken = default);
    }
}