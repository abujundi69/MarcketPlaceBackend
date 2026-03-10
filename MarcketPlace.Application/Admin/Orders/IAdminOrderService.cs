using MarcketPlace.Application.Admin.Orders.Dtos;

namespace MarcketPlace.Application.Admin.Orders
{
    public interface IAdminOrderService
    {
        Task<IReadOnlyList<AdminOrderListItemDto>> GetAllAsync(
            CancellationToken cancellationToken = default);
    }
}