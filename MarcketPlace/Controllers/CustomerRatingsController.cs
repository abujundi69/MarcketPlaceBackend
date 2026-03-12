using System.Security.Claims;
using MarcketPlace.Application.Customer.Ratings;
using MarcketPlace.Application.Customer.Ratings.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/customer/orders")]
    [Authorize(Roles = nameof(UserRole.Customer))]
    public class CustomerRatingsController : ControllerBase
    {
        private readonly ICustomerRatingService _customerRatingService;

        public CustomerRatingsController(ICustomerRatingService customerRatingService)
        {
            _customerRatingService = customerRatingService;
        }

        [HttpGet("{orderId:int}/rating-availability")]
        public async Task<ActionResult<CustomerOrderRatingAvailabilityDto>> GetRatingAvailability(
            int orderId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _customerRatingService.GetOrderRatingAvailabilityAsync(
                userId.Value,
                orderId,
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("{orderId:int}/driver-rating")]
        public async Task<ActionResult<CustomerDriverRatingDto>> CreateDriverRating(
            int orderId,
            [FromBody] CreateDriverRatingDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _customerRatingService.CreateDriverRatingAsync(
                userId.Value,
                orderId,
                dto,
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("{orderId:int}/stores/{storeId:int}/rating")]
        public async Task<ActionResult<CustomerStoreRatingDto>> CreateStoreRating(
            int orderId,
            int storeId,
            [FromBody] CreateStoreRatingDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _customerRatingService.CreateStoreRatingAsync(
                userId.Value,
                orderId,
                storeId,
                dto,
                cancellationToken);

            return Ok(result);
        }

        private int? GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdValue))
                return null;

            return int.TryParse(userIdValue, out var userId)
                ? userId
                : null;
        }
    }
}