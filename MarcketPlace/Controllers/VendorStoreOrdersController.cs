using MarcketPlace.Application.Vendor.Orders;
using MarcketPlace.Application.Vendor.Orders.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/vendor/stores/{storeId:int}/orders")]
    public class VendorStoreOrdersController : ControllerBase
    {
        private readonly IVendorStoreOrderService _vendorStoreOrderService;

        public VendorStoreOrdersController(IVendorStoreOrderService vendorStoreOrderService)
        {
            _vendorStoreOrderService = vendorStoreOrderService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<VendorStoreOrderListItemDto>>> GetStoreOrders(
            [FromRoute] int storeId,
            [FromQuery] int vendorUserId,
            CancellationToken cancellationToken)
        {
            var result = await _vendorStoreOrderService.GetStoreOrdersAsync(
                vendorUserId,
                storeId,
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("{orderStoreId:int}")]
        public async Task<ActionResult<VendorStoreOrderDetailsDto>> GetStoreOrderDetails(
            [FromRoute] int storeId,
            [FromRoute] int orderStoreId,
            [FromQuery] int vendorUserId,
            CancellationToken cancellationToken)
        {
            var result = await _vendorStoreOrderService.GetStoreOrderDetailsAsync(
                vendorUserId,
                storeId,
                orderStoreId,
                cancellationToken);

            return Ok(result);
        }

        [HttpPatch("{orderStoreId:int}/status")]
        public async Task<ActionResult<VendorStoreOrderDetailsDto>> UpdateStoreOrderStatus(
            [FromRoute] int storeId,
            [FromRoute] int orderStoreId,
            [FromQuery] int vendorUserId,
            [FromBody] UpdateVendorStoreOrderStatusDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _vendorStoreOrderService.UpdateStoreOrderStatusAsync(
                vendorUserId,
                storeId,
                orderStoreId,
                dto,
                cancellationToken);

            return Ok(result);
        }
    }
}