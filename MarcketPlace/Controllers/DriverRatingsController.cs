using System.Security.Claims;
using MarcketPlace.Application.Driver.Ratings;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.API.Controllers.Driver
{
    [ApiController]
    [Route("api/driver/ratings")]
    [Authorize(Roles = nameof(UserRole.Driver))]
    public class DriverRatingsController : ControllerBase
    {
        private readonly IDriverRatingService _service;

        public DriverRatingsController(IDriverRatingService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyRatings(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _service.GetMyRatingsAsync(userId, cancellationToken);
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
