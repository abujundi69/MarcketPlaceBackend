using MarcketPlace.Application.Admin.Products;
using MarcketPlace.Application.Admin.Products.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    public class AdminProductsController : ControllerBase
    {
        private readonly IProductAdminService _productAdminService;

        public AdminProductsController(IProductAdminService productAdminService)
        {
            _productAdminService = productAdminService;
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create(
            [FromBody] CreateProductDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _productAdminService.CreateAsync(dto, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductDto>> Update(
            int id,
            [FromBody] UpdateProductDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _productAdminService.UpdateAsync(id, dto, cancellationToken);

            if (result is null)
                return NotFound(new { message = $"Product with id {id} was not found." });

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _productAdminService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _productAdminService.GetByIdAsync(id, cancellationToken);

            if (result is null)
                return NotFound(new { message = $"Product with id {id} was not found." });

            return Ok(result);
        }

        [HttpGet("category/{categoryId:int}")]
        public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetByCategory(int categoryId, CancellationToken cancellationToken)
        {
            var result = await _productAdminService.GetByCategoryAsync(categoryId, cancellationToken);
            return Ok(result);
        }
    }
}