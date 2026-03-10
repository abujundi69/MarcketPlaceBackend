using MarcketPlace.Application.Vendor.Categories;
using MarcketPlace.Application.Vendor.Categories.Dtos;
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
        private readonly IVendorCategoryService _vendorCategoryService;

        public VendorCategoriesController(IVendorCategoryService vendorCategoryService)
        {
            _vendorCategoryService = vendorCategoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<VendorCategoryListItemDto>>> GetActiveCategories(
            CancellationToken cancellationToken)
        {
            var result = await _vendorCategoryService.GetActiveCategoriesAsync(cancellationToken);
            return Ok(result);
        }
    }
}
