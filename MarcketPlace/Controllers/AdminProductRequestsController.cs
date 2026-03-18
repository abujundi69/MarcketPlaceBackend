using System.Security.Claims;
using MarcketPlace.Application.Admin.ProductRequests;
using MarcketPlace.Application.Admin.ProductRequests.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/admin/product-requests")]
    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    public class AdminProductRequestsController : ControllerBase
    {
        private readonly IAdminProductRequestService _service;

        public AdminProductRequestsController(IAdminProductRequestService service)
        {
            _service = service;
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IReadOnlyList<AdminProductRequestDto>>> GetPending(CancellationToken cancellationToken)
        {
            var result = await _service.GetPendingAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdminProductRequestDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost("{id:int}/approve")]
        public async Task<ActionResult<AdminProductRequestDto>> Approve(
            int id,
            CancellationToken cancellationToken)
        {
            var adminUserId = GetCurrentUserId();
            var result = await _service.ApproveAsync(id, adminUserId, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id:int}/reject")]
        public async Task<ActionResult<AdminProductRequestDto>> Reject(
            int id,
            [FromBody] RejectProductRequestDto dto,
            CancellationToken cancellationToken)
        {
            var adminUserId = GetCurrentUserId();
            var result = await _service.RejectAsync(id, adminUserId, dto ?? new RejectProductRequestDto(), cancellationToken);
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
