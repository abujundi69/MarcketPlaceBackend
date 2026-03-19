using MarcketPlace.Application.Auth.Dtos;

namespace MarcketPlace.Application.Auth
{
    public interface IAuthService
    {
        Task<LoginResultDto> CustomerRegisterAsync(
            CustomerRegisterRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<LoginResultDto> LoginAsync(
            LoginRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<AuthResponseDto> VerifyCustomerOtpAsync(
            VerifyCustomerOtpRequestDto dto,
            CancellationToken cancellationToken = default);
    }
}