using System.Security.Claims;
using MarcketPlace.Application.Customer.Locations;
using MarcketPlace.Application.Customer.Locations.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/customer/location")]
    [Authorize(Roles = nameof(UserRole.Customer))]
    public class CustomerLocationController : ControllerBase
    {
        private readonly ICustomerLocationService _customerLocationService;

        public CustomerLocationController(ICustomerLocationService customerLocationService)
        {
            _customerLocationService = customerLocationService;
        }

        [HttpGet]
        public async Task<ActionResult<CustomerSavedLocationDto>> GetMyLocation(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _customerLocationService.GetMyLocationAsync(
                userId.Value,
                cancellationToken);

            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<CustomerSavedLocationDto>> UpdateMyLocation(
            [FromBody] UpdateCustomerSavedLocationDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _customerLocationService.UpdateMyLocationAsync(
                userId.Value,
                dto,
                cancellationToken);

            return Ok(result);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}