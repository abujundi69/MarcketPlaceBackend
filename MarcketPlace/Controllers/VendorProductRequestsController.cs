using MarcketPlace.Application.Vendor.ProductRequests;
using MarcketPlace.Application.Vendor.ProductRequests.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/vendor/product-requests")]
    public class VendorProductRequestsController : ControllerBase
    {
        private readonly IVendorProductRequestService _service;

        public VendorProductRequestsController(IVendorProductRequestService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<VendorProductRequestDto>> Create(
            [FromQuery] int vendorId,
            [FromBody] CreateVendorProductRequestDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(vendorId, dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<VendorProductRequestDto>>> GetMyRequests(
            [FromQuery] int vendorId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetMyRequestsAsync(vendorId, cancellationToken);
            return Ok(result);
        }
    }
}