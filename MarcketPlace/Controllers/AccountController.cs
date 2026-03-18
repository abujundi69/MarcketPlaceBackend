using System.Security.Claims;
using MarcketPlace.Application.Account;
using MarcketPlace.Application.Account.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/account")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IMyAccountService _myAccountService;

        public AccountController(IMyAccountService myAccountService)
        {
            _myAccountService = myAccountService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<MyProfileDto>> GetMyProfile(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _myAccountService.GetMyProfileAsync(userId.Value, cancellationToken);
            return Ok(result);
        }

        [HttpPut("me")]
        public async Task<ActionResult<MyProfileDto>> UpdateMyProfile(
            [FromBody] UpdateMyProfileDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _myAccountService.UpdateMyProfileAsync(userId.Value, dto, cancellationToken);
            return Ok(result);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangeMyPasswordDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            await _myAccountService.ChangePasswordAsync(userId.Value, dto, cancellationToken);
            return Ok(new { message = "تم تغيير كلمة المرور بنجاح." });
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("sub")?.Value;

            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}