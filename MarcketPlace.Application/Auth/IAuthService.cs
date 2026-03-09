using MarcketPlace.Application.Auth.Dtos;

namespace MarcketPlace.Application.Auth
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
    }
}