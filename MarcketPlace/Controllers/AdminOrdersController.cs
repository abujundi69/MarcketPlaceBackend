using MarcketPlace.Application.Admin.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IAdminOrderService _service;

        public AdminOrdersController(IAdminOrderService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _service.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{orderId:int}")]
        public async Task<IActionResult> GetById(int orderId, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(orderId, cancellationToken);
            return Ok(result);
        }
    }
}