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
            var result = await _service.CreateAsync(userId, dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<VendorProductRequestDto>>> GetMine(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _service.GetMineAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<VendorProductRequestDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _service.GetByIdAsync(userId, id, cancellationToken);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(raw) || !int.TryParse(raw, out var userId))
                throw new UnauthorizedAccessException("المستخدم غير مسجل الدخول.");
            return userId;
        }
    }
}
