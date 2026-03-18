using System.Security.Claims;
using MarcketPlace.Application.Driver.Orders;
using MarcketPlace.Application.Shared.Orders.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.API.Controllers.Driver
{
    [ApiController]
    [Route("api/driver/orders")]
    [Authorize(Roles = nameof(UserRole.Driver))]
    public class DriverOrdersController : ControllerBase
    {
        private readonly IDriverOrderService _service;

        public DriverOrdersController(IDriverOrderService service)
        {
            _service = service;
        }

        [HttpGet("available")]
        public async Task<ActionResult<IReadOnlyList<OrderListItemDto>>> GetAvailable(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _service.GetAvailableOrdersAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("mine")]
        public async Task<ActionResult<IReadOnlyList<OrderListItemDto>>> GetMine(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _service.GetMyOrdersAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{orderId:int}")]
        public async Task<ActionResult<OrderDetailsDto>> GetById(
            int orderId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _service.GetByIdAsync(userId, orderId, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{orderId:int}/accept")]
        public async Task<ActionResult<OrderDetailsDto>> Accept(
            int orderId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _service.AcceptOrderAsync(userId, orderId, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{orderId:int}/picked-up")]
        public async Task<ActionResult<OrderDetailsDto>> MarkPickedUp(
            int orderId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _service.MarkPickedUpAsync(userId, orderId, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{orderId:int}/delivered")]
        public async Task<ActionResult<OrderDetailsDto>> MarkDelivered(
            int orderId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _service.MarkDeliveredAsync(userId, orderId, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{orderId:int}/cancel-assignment")]
        public async Task<IActionResult> CancelAssignment(
            int orderId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            await _service.CancelAssignmentAsync(userId, orderId, cancellationToken);
            return NoContent();
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