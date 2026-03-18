using MarcketPlace.Application.Admin.Categories;
using MarcketPlace.Application.Admin.Categories.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/categories")]
    [Authorize(Roles = "SuperAdmin")]
    public class AdminCategoriesController : ControllerBase
    {
        private readonly IAdminCategoryService _categoryService;

        public AdminCategoriesController(IAdminCategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<ActionResult<AdminCategoryDto>> Create(
            [FromBody] CreateCategoryDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _categoryService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AdminCategoryListItemDto>>> GetAll(
            CancellationToken cancellationToken)
        {
            var result = await _categoryService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdminCategoryDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _categoryService.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<AdminCategoryDto>> Update(
            int id,
            [FromBody] UpdateCategoryDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _categoryService.UpdateAsync(id, dto, cancellationToken);
            return Ok(result);
        }
    }
}