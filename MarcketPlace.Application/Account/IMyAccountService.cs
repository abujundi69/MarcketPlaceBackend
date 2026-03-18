using MarcketPlace.Application.Account.Dtos;

namespace MarcketPlace.Application.Account
{
    public interface IMyAccountService
    {
        Task<MyProfileDto> GetMyProfileAsync(int userId, CancellationToken cancellationToken = default);
        Task<MyProfileDto> UpdateMyProfileAsync(int userId, UpdateMyProfileDto dto, CancellationToken cancellationToken = default);
        Task ChangePasswordAsync(int userId, ChangeMyPasswordDto dto, CancellationToken cancellationToken = default);
    }
}