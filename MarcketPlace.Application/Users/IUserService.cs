using MarcketPlace.Application.Users.Dtos;

namespace MarcketPlace.Application.Users
{
    public interface IUserService
    {
        Task<IReadOnlyList<UserListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<UserListItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task ChangePasswordByAdminAsync(
            int userId,
            AdminChangeUserPasswordDto dto,
            CancellationToken cancellationToken = default);
    }

}
