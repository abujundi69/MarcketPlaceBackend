using MarcketPlace.Application.Admin.SystemSettings;
using MarcketPlace.Application.Admin.SystemSettings.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/admin/system-settings")]
    public class AdminSystemSettingsController : ControllerBase
    {
        private readonly ISystemSettingAdminService _service;

        public AdminSystemSettingsController(ISystemSettingAdminService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<SystemSettingDto>> Get(CancellationToken cancellationToken)
        {
            var result = await _service.GetAsync(cancellationToken);
            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<SystemSettingDto>> Update(
            [FromBody] UpdateSystemSettingDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(dto, cancellationToken);
            return Ok(result);
        }
    }
}