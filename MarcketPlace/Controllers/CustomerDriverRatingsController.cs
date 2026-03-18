using System.Security.Claims;
using MarcketPlace.Application.Customer.DriverRatings;
using MarcketPlace.Application.Customer.DriverRatings.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Api.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/driver-ratings")]
    [Authorize(Roles = "Customer")]
    public class CustomerDriverRatingsController : ControllerBase
    {
        private readonly ICustomerDriverRatingService _driverRatingService;

        public CustomerDriverRatingsController(ICustomerDriverRatingService driverRatingService)
        {
            _driverRatingService = driverRatingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyRatings(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _driverRatingService.GetMyRatingsAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult> GetByOrder(int orderId, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _driverRatingService.GetByOrderAsync(userId, orderId, cancellationToken);
            return Ok(result);
        }

        [HttpPost("order/{orderId:int}")]
        public async Task<IActionResult> RateOrder(
            int orderId,
            [FromBody] CreateCustomerDriverRatingDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _driverRatingService.RateOrderDriverAsync(userId, orderId, dto, cancellationToken);
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