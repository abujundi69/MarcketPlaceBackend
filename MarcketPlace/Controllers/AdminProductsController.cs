using MarcketPlace.Application.Admin.Products;
using MarcketPlace.Application.Admin.Products.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = "SuperAdmin")]
    public class AdminProductsController : ControllerBase
    {
        private readonly IAdminProductService _productService;

        public AdminProductsController(IAdminProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<ActionResult<AdminProductDto>> Create(
            [FromBody] CreateAdminProductDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _productService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AdminProductDto>>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _productService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("category/{categoryId:int}")]
        public async Task<ActionResult<IReadOnlyList<AdminProductDto>>> GetByCategory(
            int categoryId,
            CancellationToken cancellationToken)
        {
            var result = await _productService.GetByCategoryAsync(categoryId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<AdminProductDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await _productService.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<AdminProductDto>> Update(
            int id,
            [FromBody] UpdateAdminProductDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _productService.UpdateAsync(id, dto, cancellationToken);
            return Ok(result);
        }
    }
}