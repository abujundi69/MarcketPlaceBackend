using MarcketPlace.Application.Auth;
using MarcketPlace.Application.Auth.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResultDto>> Login(
            [FromBody] LoginRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpPost("verify-first-login-otp")]
        public async Task<ActionResult<AuthResponseDto>> VerifyFirstLoginOtp(
            [FromBody] VerifyFirstLoginOtpRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.VerifyFirstLoginOtpAsync(dto, cancellationToken);
            return Ok(result);
        }
    }
}