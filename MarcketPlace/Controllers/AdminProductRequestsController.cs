using MarcketPlace.Application.Admin.ProductRequests;
using MarcketPlace.Application.Admin.ProductRequests.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/admin/product-requests")]
    public class AdminProductRequestsController : ControllerBase
    {
        private readonly IAdminProductRequestService _service;

        public AdminProductRequestsController(IAdminProductRequestService service)
        {
            _service = service;
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IReadOnlyList<AdminProductRequestListItemDto>>> GetPending(
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPendingAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdminProductRequestDetailsDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);

            if (result is null)
                return NotFound(new { message = $"طلب المنتج ذو الرقم {id} غير موجود." });

            return Ok(result);
        }

        [HttpPost("{id:int}/approve")]
        public async Task<ActionResult<AdminProductRequestDetailsDto>> Approve(
            int id,
            [FromQuery] int? adminUserId,
            [FromBody] ReviewProductRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.ApproveAsync(id, adminUserId, dto.Note, cancellationToken);

            if (result is null)
                return NotFound(new { message = $"طلب المنتج ذو الرقم {id} غير موجود." });

            return Ok(result);
        }

        [HttpPost("{id:int}/reject")]
        public async Task<ActionResult<AdminProductRequestDetailsDto>> Reject(
            int id,
            [FromQuery] int? adminUserId,
            [FromBody] ReviewProductRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.RejectAsync(id, adminUserId, dto.Note, cancellationToken);

            if (result is null)
                return NotFound(new { message = $"طلب المنتج ذو الرقم {id} غير موجود." });

            return Ok(result);
        }
    }
}