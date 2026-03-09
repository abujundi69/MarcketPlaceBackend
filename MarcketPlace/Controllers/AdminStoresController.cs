using MarcketPlace.Application.Admin.Stores;
using MarcketPlace.Application.Admin.Stores.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/admin/stores")]
    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    public class AdminStoresController : ControllerBase
    {
        private readonly IStoreAdminService _storeAdminService;

        public AdminStoresController(IStoreAdminService storeAdminService)
        {
            _storeAdminService = storeAdminService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<StoreAdminListItemDto>>> GetAllStores(
            CancellationToken cancellationToken)
        {
            var result = await _storeAdminService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<StoreAdminDetailsDto>> GetStoreById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _storeAdminService.GetByIdAsync(id, cancellationToken);

            if (result is null)
                return NotFound(new { message = "المتجر غير موجود." });

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<StoreAdminDetailsDto>> CreateStore(
            [FromBody] CreateStoreByAdminDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _storeAdminService.CreateAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetStoreById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<StoreAdminDetailsDto>> UpdateStore(
            int id,
            [FromBody] UpdateStoreByAdminDto dto,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _storeAdminService.UpdateAsync(id, dto, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}