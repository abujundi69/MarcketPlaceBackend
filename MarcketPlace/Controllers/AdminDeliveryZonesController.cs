using MarcketPlace.Application.Admin.DeliveryZones;
using MarcketPlace.Application.Admin.DeliveryZones.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/admin/delivery-zones")]
    public class AdminDeliveryZonesController : ControllerBase
    {
        private readonly IDeliveryZoneAdminService _service;

        public AdminDeliveryZonesController(IDeliveryZoneAdminService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<DeliveryZoneDto>> Create(
            [FromBody] CreateDeliveryZoneDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<DeliveryZoneDto>>> GetAll(
            CancellationToken cancellationToken)
        {
            var result = await _service.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<DeliveryZoneDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);

            if (result is null)
                return NotFound(new { message = $"المنطقة ذات الرقم {id} غير موجودة." });

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<DeliveryZoneDto>> Update(
            int id,
            [FromBody] UpdateDeliveryZoneDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(id, dto, cancellationToken);
            return Ok(result);
        }
    }
}