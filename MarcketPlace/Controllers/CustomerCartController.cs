using System.Security.Claims;
using MarcketPlace.Application.Customer.Cart;
using MarcketPlace.Application.Customer.Cart.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/cart")]
    [Authorize(Roles = "Customer")]
    public class CustomerCartController : ControllerBase
    {
        private readonly ICustomerCartService _cartService;

        public CustomerCartController(ICustomerCartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<ActionResult<CustomerCartDto>> GetCart(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.GetCartAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpPost("items")]
        public async Task<ActionResult<CustomerCartDto>> AddItem(
            [FromBody] AddCustomerCartItemDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.AddItemAsync(userId, dto, cancellationToken);
            return Ok(result);
        }

        [HttpPut("items/{cartItemId:int}")]
        public async Task<ActionResult<CustomerCartDto>> UpdateItem(
            int cartItemId,
            [FromBody] UpdateCustomerCartItemDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.UpdateItemAsync(userId, cartItemId, dto, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("items/{cartItemId:int}")]
        public async Task<ActionResult<CustomerCartDto>> RemoveItem(
            int cartItemId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.RemoveItemAsync(userId, cartItemId, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("clear")]
        public async Task<ActionResult<CustomerCartDto>> Clear(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.ClearAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpPost("reorder-last")]
        public async Task<ActionResult<CustomerCartDto>> ReorderLast(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _cartService.ReorderLastOrderAsync(userId, cancellationToken);
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