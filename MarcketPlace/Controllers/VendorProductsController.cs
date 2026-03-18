using System.Security.Claims;
using MarcketPlace.Application.Vendor.Products;
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
        private readonly IVendorProductService _service;

        public VendorProductsController(IVendorProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<MarcketPlace.Application.Vendor.Products.Dtos.VendorProductDto>>> GetMyProducts(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _service.GetMyProductsAsync(userId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MarcketPlace.Application.Vendor.Products.Dtos.VendorProductDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _service.GetByIdAsync(userId, id, cancellationToken);
            if (result is null)
                return NotFound();
            return Ok(result);
        }

        private int GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(raw) || !int.TryParse(raw, out var userId))
                throw new UnauthorizedAccessException("المستخدم غير مسجل الدخول.");
            return userId;
        }
    }
}
