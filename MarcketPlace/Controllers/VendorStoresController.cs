using System.Security.Claims;
using MarcketPlace.Application.Vendor.Stores;
using MarcketPlace.Application.Vendor.Stores.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/vendor/stores")]
    [Authorize(Roles = nameof(UserRole.Vendor))]
    public class VendorStoresController : ControllerBase
    {
        private readonly IVendorStoreService _vendorStoreService;

        public VendorStoresController(IVendorStoreService vendorStoreService)
        {
            _vendorStoreService = vendorStoreService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<VendorStoreListItemDto>>> GetMyStores(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized();

            var result = await _vendorStoreService.GetMyStoresAsync(userId.Value, cancellationToken);
            return Ok(result);
        }

        private int? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }
}
