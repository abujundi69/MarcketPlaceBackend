using MarcketPlace.Application.Admin.Drivers.Dtos;

namespace MarcketPlace.Application.Admin.Drivers
{
    public interface IDriverAdminService
    {
        Task<IReadOnlyList<DriverListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<DriverDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<DriverDetailsDto> CreateAsync(CreateDriverDto dto, CancellationToken cancellationToken = default);
        Task<DriverDetailsDto> UpdateAsync(int id, UpdateDriverDto dto, CancellationToken cancellationToken = default);
        Task<DriverDetailsDto> UpdateStatusAsync(int id, UpdateDriverStatusDto dto, CancellationToken cancellationToken = default);
    }
}