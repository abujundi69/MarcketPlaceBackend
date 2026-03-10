using MarcketPlace.Application.Orders.Dtos;

namespace MarcketPlace.Application.Orders
{
    public interface IOrderService
    {
        Task<CheckoutOrderResultDto> CheckoutFromCartAsync(
            CheckoutOrderDto dto,
            CancellationToken cancellationToken = default);
    }
}