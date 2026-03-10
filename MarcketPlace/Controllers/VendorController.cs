using System.Security.Claims;
using MarcketPlace.Application.Vendor.Products;
using MarcketPlace.Application.Vendor.Products.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/vendor/products")]
    [Authorize(Roles = nameof(UserRole.Vendor))]
    public class VendorProductsController : ControllerBase
    {
        private readonly IVendorProductService _vendorProductService;

        public VendorProductsController(IVendorProductService vendorProductService)
        {
            _vendorProductService = vendorProductService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<VendorProductDto>>> GetMyProducts(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _vendorProductService.GetAllByUserAsync(userId.Value, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<VendorProductDto>> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _vendorProductService.GetByIdByUserAsync(userId.Value, id, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{productId:int}")]
        public async Task<ActionResult<VendorProductDto>> Update(
            int productId,
            [FromBody] UpdateVendorProductDto dto,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _vendorProductService.UpdateByUserAsync(userId.Value, productId, dto, cancellationToken);
            return Ok(result);
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }
}