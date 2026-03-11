using System.Security.Claims;
using MarcketPlace.Application.Customer.Cart;
using MarcketPlace.Application.Customer.Cart.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/customer/cart")]
    [Authorize(Roles = nameof(UserRole.Customer))]
    public class CustomerCartController : ControllerBase
    {
        private readonly ICustomerCartService _customerCartService;

        public CustomerCartController(ICustomerCartService customerCartService)
        {
            _customerCartService = customerCartService;
        }

        [HttpGet]
        public async Task<ActionResult<CustomerCartDto>> GetCart(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _customerCartService.GetCartAsync(userId.Value, cancellationToken);
            return Ok(result);
        }

        [HttpPost("items")]
        public async Task<ActionResult<CustomerCartDto>> AddItem(
            [FromBody] AddCustomerCartItemDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _customerCartService.AddItemAsync(userId.Value, dto, cancellationToken);
            return Ok(result);
        }

        [HttpPut("items/{cartItemId:int}")]
        public async Task<ActionResult<CustomerCartDto>> UpdateQuantity(
            int cartItemId,
            [FromBody] UpdateCustomerCartItemQuantityDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _customerCartService.UpdateQuantityAsync(
                userId.Value,
                cartItemId,
                dto,
                cancellationToken);

            return Ok(result);
        }

        [HttpDelete("items/{cartItemId:int}")]
        public async Task<IActionResult> RemoveItem(
            int cartItemId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            await _customerCartService.RemoveItemAsync(userId.Value, cartItemId, cancellationToken);
            return NoContent();
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            await _customerCartService.ClearCartAsync(userId.Value, cancellationToken);
            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}