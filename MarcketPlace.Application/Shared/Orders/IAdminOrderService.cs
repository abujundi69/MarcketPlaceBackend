using MarcketPlace.Application.Shared.Orders.Dtos;

namespace MarcketPlace.Application.Admin.Orders
{
    public interface IAdminOrderService
    {
        Task<IReadOnlyList<OrderListItemDto>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<OrderDetailsDto> GetByIdAsync(
            int orderId,
            CancellationToken cancellationToken = default);
    }
}