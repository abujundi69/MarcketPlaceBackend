using MarcketPlace.Application.Admin.Orders;
using MarcketPlace.Application.Admin.Orders.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/admin/orders")]
    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IAdminOrderService _adminOrderService;

        public AdminOrdersController(IAdminOrderService adminOrderService)
        {
            _adminOrderService = adminOrderService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AdminOrderListItemDto>>> GetAll(
            CancellationToken cancellationToken)
        {
            var result = await _adminOrderService.GetAllAsync(cancellationToken);
            return Ok(result);
        }
    }
}