using MarcketPlace.Application.Admin.Dashboard.Dtos;

namespace MarcketPlace.Application.Admin.Dashboard
{
    public interface IAdminDashboardService
    {
        Task<SuperAdminDashboardStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
    }
}