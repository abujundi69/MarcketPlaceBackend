using MarcketPlace.Application.Users.Dtos;

namespace MarcketPlace.Application.Users
{
    public interface IUserService
    {
        Task<IReadOnlyList<UserListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}