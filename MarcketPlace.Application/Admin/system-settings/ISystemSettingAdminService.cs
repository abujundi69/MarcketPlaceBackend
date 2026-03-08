using MarcketPlace.Application.Admin.SystemSettings.Dtos;

namespace MarcketPlace.Application.Admin.SystemSettings
{
    public interface ISystemSettingAdminService
    {
        Task<SystemSettingDto> GetAsync(CancellationToken cancellationToken = default);
        Task<SystemSettingDto> UpdateAsync(UpdateSystemSettingDto dto, CancellationToken cancellationToken = default);
    }
}   