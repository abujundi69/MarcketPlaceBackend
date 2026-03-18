using System.Security.Claims;
using MarcketPlace.Application.Customer.Favorites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Api.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/favorites")]
    [Authorize(Roles = "Customer")]
    public class CustomerFavoritesController : ControllerBase
    {
        private readonly ICustomerFavoriteService _favoriteService;

        public CustomerFavoritesController(ICustomerFavoriteService favoriteService)
        {
            _favoriteService = favoriteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyFavorites(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _favoriteService.GetMyFavoritesAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{productId:int}")]
        public async Task<IActionResult> Add(int productId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _favoriteService.AddAsync(userId, productId, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{productId:int}")]
        public async Task<IActionResult> Remove(int productId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _favoriteService.RemoveAsync(userId, productId, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{productId:int}/toggle")]
        public async Task<IActionResult> Toggle(int productId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _favoriteService.ToggleAsync(userId, productId, cancellationToken);
            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(value, out var userId))
                throw new UnauthorizedAccessException("المستخدم غير مصادق عليه.");

            return userId;
        }
    }
}