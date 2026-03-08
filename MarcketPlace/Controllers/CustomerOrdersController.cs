using MarcketPlace.Application.Orders;
using MarcketPlace.Application.Orders.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/customer/orders")]
    public class CustomerOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public CustomerOrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutOrderResultDto>> Checkout(
            [FromBody] CheckoutOrderDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _orderService.CheckoutFromCartAsync(dto, cancellationToken);
            return Ok(result);
        }
    }
}