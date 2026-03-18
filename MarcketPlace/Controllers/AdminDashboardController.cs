using MarcketPlace.Application.Admin.Dashboard;
using MarcketPlace.Application.Admin.Dashboard.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<SuperAdminDashboardStatsDto>> GetStats(CancellationToken cancellationToken)
        {
            var result = await _adminDashboardService.GetStatsAsync(cancellationToken);
            return Ok(result);
        }
    }
}