using System.Security.Claims;
using MarcketPlace.Application.Customer.Favorites;
using MarcketPlace.Application.Customer.Favorites.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/customer/favorites")]
    [Authorize(Roles = nameof(UserRole.Customer))]
    public class CustomerFavoritesController : ControllerBase
    {
        private readonly ICustomerFavoritesService _customerFavoritesService;

        public CustomerFavoritesController(ICustomerFavoritesService customerFavoritesService)
        {
            _customerFavoritesService = customerFavoritesService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CustomerFavoriteProductDto>>> GetFavorites(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _customerFavoritesService.GetFavoritesAsync(userId.Value, cancellationToken);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddFavorite(
            [FromBody] AddCustomerFavoriteDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            await _customerFavoritesService.AddFavoriteAsync(userId.Value, dto.ProductId, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> RemoveFavorite(
            int productId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            await _customerFavoritesService.RemoveFavoriteAsync(userId.Value, productId, cancellationToken);
            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}
