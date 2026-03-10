using MarcketPlace.Application.Admin.Categories;
using MarcketPlace.Application.Admin.Categories.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/admin/categories")]
    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly ICategoryAdminService _categoryAdminService;

        public AdminCategoriesController(ICategoryAdminService categoryAdminService)
        {
            _categoryAdminService = categoryAdminService;
        }

        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create(
            [FromBody] CreateCategoryDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _categoryAdminService.CreateAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _categoryAdminService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _categoryAdminService.GetByIdAsync(id, cancellationToken);

            if (result is null)
                return NotFound(new { message = $"Category with id {id} was not found." });

            return Ok(result);
        }
    }
}