using MarcketPlace.Application.Auth.Dtos;

namespace MarcketPlace.Application.Auth
{
    public interface IAuthService
    {
        Task<LoginResultDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> VerifyFirstLoginOtpAsync(VerifyFirstLoginOtpRequestDto dto, CancellationToken cancellationToken = default);
    }
}