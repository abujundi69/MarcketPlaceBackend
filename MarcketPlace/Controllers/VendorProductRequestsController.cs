using System.Security.Claims;
using MarcketPlace.Application.Vendor.ProductRequests;
using MarcketPlace.Application.Vendor.ProductRequests.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/vendor/product-requests")]
    [Authorize(Roles = nameof(UserRole.Vendor))]
    public class VendorProductRequestsController : ControllerBase
    {
        private readonly IVendorProductRequestService _service;

        public VendorProductRequestsController(IVendorProductRequestService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<VendorProductRequestDto>> Create(
            [FromBody] CreateVendorProductRequestDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _service.CreateByUserAsync(userId.Value, dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<VendorProductRequestDto>>> GetMyRequests(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _service.GetMyRequestsByUserAsync(userId.Value, cancellationToken);
            return Ok(result);
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }
}