using System.Security.Claims;
using MarcketPlace.Application.Customer.Orders;
using MarcketPlace.Application.Customer.Orders.Dtos;
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
        private readonly ICustomerOrderService _customerOrderService;

        public CustomerOrdersController(ICustomerOrderService customerOrderService)
        {
            _customerOrderService = customerOrderService;
        }

        [HttpPost("checkout-from-cart")]
        public async Task<ActionResult<CustomerCreatedOrderDto>> CheckoutFromCart(
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

        [HttpGet("my-orders")]
        public async Task<ActionResult<IReadOnlyList<CustomerOrderListItemDto>>> GetMyOrders(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _customerOrderService.GetMyOrdersAsync(
                userId.Value,
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("{orderId:int}")]
        public async Task<ActionResult<CustomerOrderDetailsDto>> GetById(
            int orderId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _customerOrderService.GetByIdAsync(
                userId.Value,
                orderId,
                cancellationToken);

            return Ok(result);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
        [HttpPost("{orderId:int}/cancel")]
        public async Task<ActionResult<CustomerOrderDetailsDto>> Cancel(
        int orderId,
            [FromBody] CancelCustomerOrderDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _customerOrderService.CancelAsync(
                userId.Value,
                orderId,
                dto,
                cancellationToken);

            return Ok(result);
        }
    }
}