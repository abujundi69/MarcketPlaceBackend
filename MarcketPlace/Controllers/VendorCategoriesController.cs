using MarcketPlace.Application.Customer.Catalog;
using MarcketPlace.Application.Customer.Catalog.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/vendor/categories")]
    [Authorize(Roles = nameof(UserRole.Vendor))]
    public class VendorCategoriesController : ControllerBase
    {
        private readonly ICustomerCatalogService _catalogService;

        public VendorCategoriesController(ICustomerCatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CustomerCategoryDto>>> GetCategories(CancellationToken cancellationToken)
        {
            var result = await _catalogService.GetCategoriesAsync(cancellationToken);
            return Ok(result);
        }
    }
}
