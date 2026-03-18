using System.Security.Claims;
using MarcketPlace.Application.Customer.Orders;
using MarcketPlace.Application.Customer.Orders.Dtos;
using MarcketPlace.Application.Shared.Orders.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/orders")]
    [Authorize(Roles = nameof(UserRole.Customer))]
    public class CustomerOrdersController : ControllerBase
    {
        private readonly ICustomerOrderService _orderService;

        public CustomerOrdersController(ICustomerOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("from-cart")]
        public async Task<ActionResult<CustomerCreatedOrderDto>> CreateFromCart(
            [FromBody] CreateCustomerOrderFromCartDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _orderService.CreateFromCartAsync(userId, dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<OrderListItemDto>>> GetMyOrders(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _orderService.GetMyOrdersAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{orderId:int}")]
        public async Task<ActionResult<OrderDetailsDto>> GetById(
            int orderId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _orderService.GetByIdAsync(userId, orderId, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{orderId:int}/cancel")]
        public async Task<ActionResult<OrderDetailsDto>> Cancel(
            int orderId,
            [FromBody] CancelCustomerOrderDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _orderService.CancelAsync(userId, orderId, dto, cancellationToken);
            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");

            if (!int.TryParse(raw, out var userId))
                throw new UnauthorizedAccessException("المستخدم الحالي غير صالح.");

            return userId;
        }
    }
}