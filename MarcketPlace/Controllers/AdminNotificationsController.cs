using System.Security.Claims;
using MarcketPlace.Application.Admin.Notifications;
using MarcketPlace.Application.Admin.Notifications.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/admin/notifications")]
    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    public class AdminNotificationsController : ControllerBase
    {
        private readonly IAdminNotificationService _notificationService;

        public AdminNotificationsController(IAdminNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AdminNotificationDto>>> GetMyNotifications(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _notificationService.GetForUserAsync(userId.Value, cancellationToken);
            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var count = await _notificationService.GetUnreadCountAsync(userId.Value, cancellationToken);
            return Ok(new { count });
        }

        [HttpPatch("{id:int}/read")]
        public async Task<ActionResult> MarkAsRead(int id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            await _notificationService.MarkAsReadAsync(id, userId.Value, cancellationToken);
            return NoContent();
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }
}
