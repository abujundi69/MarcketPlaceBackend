using MarcketPlace.Application.Customer.Catalog;
using MarcketPlace.Application.Customer.Catalog.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer")]
    public class CustomerCatalogController : ControllerBase
    {
        private readonly ICustomerCatalogService _catalogService;

        public CustomerCatalogController(ICustomerCatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IReadOnlyList<CustomerCategoryDto>>> GetCategories(
            CancellationToken cancellationToken)
        {
            var result = await _catalogService.GetCategoriesAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("categories/{categoryId:int}/products")]
        public async Task<ActionResult<IReadOnlyList<CustomerProductListItemDto>>> GetProductsByCategory(
            int categoryId,
            CancellationToken cancellationToken)
        {
            var result = await _catalogService.GetProductsByCategoryAsync(categoryId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("products/most-ordered")]
        public async Task<ActionResult<IReadOnlyList<CustomerMostOrderedProductDto>>> GetMostOrderedProducts(
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _catalogService.GetMostOrderedProductsAsync(take, cancellationToken);
            return Ok(result);
        }

        [HttpGet("products/{productId:int}")]
        public async Task<ActionResult<CustomerProductDetailsDto>> GetProductDetails(
            int productId,
            CancellationToken cancellationToken)
        {
            var result = await _catalogService.GetProductDetailsAsync(productId, cancellationToken);
            return Ok(result);
        }
    }
}