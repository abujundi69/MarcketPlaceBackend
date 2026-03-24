using MarcketPlace.Application.Auth;
using MarcketPlace.Application.Auth.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("customer/register")]
        public async Task<ActionResult<LoginResultDto>> CustomerRegister(
            [FromBody] CustomerRegisterRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.CustomerRegisterAsync(dto, cancellationToken);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResultDto>> Login(
            [FromBody] LoginRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(dto, cancellationToken);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("verify-customer-otp")]
        public async Task<ActionResult<AuthResponseDto>> VerifyCustomerOtp(
            [FromBody] VerifyCustomerOtpRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.VerifyCustomerOtpAsync(dto, cancellationToken);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<ActionResult<LoginResultDto>> ForgotPassword(
            [FromBody] ForgotPasswordRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.ForgotPasswordAsync(dto, cancellationToken);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("forgot-password/verify-otp")]
        public async Task<ActionResult<VerifyForgotPasswordOtpResultDto>> VerifyForgotPasswordOtp(
            [FromBody] VerifyForgotPasswordOtpRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.VerifyForgotPasswordOtpAsync(dto, cancellationToken);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("forgot-password/reset")]
        public async Task<ActionResult<MessageResultDto>> ResetForgotPassword(
            [FromBody] ResetForgotPasswordRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _authService.ResetForgotPasswordAsync(dto, cancellationToken);
            return Ok(result);
        }
    }
}