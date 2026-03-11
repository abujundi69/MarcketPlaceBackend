using System.Security.Claims;
using MarcketPlace.Application.Driver.Orders;
using MarcketPlace.Application.Driver.Orders.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/driver/orders")]
    [Authorize(Roles = nameof(UserRole.Driver))]
    public class DriverOrdersController : ControllerBase
    {
        private readonly IDriverOrderService _driverOrderService;

        public DriverOrdersController(IDriverOrderService driverOrderService)
        {
            _driverOrderService = driverOrderService;
        }

        [HttpGet("available")]
        public async Task<ActionResult<IReadOnlyList<DriverOrderDto>>> GetAvailableOrders(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _driverOrderService.GetAvailableOrdersAsync(
                userId.Value,
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("my-orders")]
        public async Task<ActionResult<IReadOnlyList<DriverOrderDto>>> GetMyOrders(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _driverOrderService.GetMyOrdersAsync(
                userId.Value,
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("{orderId:int}/accept")]
        public async Task<ActionResult<DriverOrderDto>> AcceptOrder(
            int orderId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _driverOrderService.AcceptOrderAsync(
                userId.Value,
                orderId,
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("{orderId:int}/pickup")]
        public async Task<ActionResult<DriverOrderDto>> PickUpOrder(
            int orderId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _driverOrderService.PickUpOrderAsync(
                userId.Value,
                orderId,
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("{orderId:int}/deliver")]
        public async Task<ActionResult<DriverOrderDto>> DeliverOrder(
            int orderId,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _driverOrderService.DeliverOrderAsync(
                userId.Value,
                orderId,
                cancellationToken);

            return Ok(result);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}