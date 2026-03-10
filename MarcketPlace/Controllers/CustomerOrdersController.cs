using System.Security.Claims;
using MarcketPlace.Application.Customer.Orders;
using MarcketPlace.Application.Customer.Orders.Dtos;
using MarcketPlace.Application.Orders;
using MarcketPlace.Application.Orders.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/customer/orders")]
    [Authorize(Roles = nameof(UserRole.Customer))]
    public class CustomerOrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerOrderService _customerOrderService;

        public CustomerOrdersController(
            IOrderService orderService,
            ICustomerOrderService customerOrderService)
        {
            _orderService = orderService;
            _customerOrderService = customerOrderService;
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutOrderResultDto>> Checkout(
            [FromBody] CheckoutOrderDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _orderService.CheckoutFromCartAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpPost("from-cart")]
        public async Task<ActionResult<CustomerCreatedOrderDto>> CreateFromCart(
            [FromBody] CreateCustomerOrderFromCartDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _customerOrderService.CreateFromCartAsync(
                userId.Value,
                dto,
                cancellationToken);

            return Ok(result);
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }
}