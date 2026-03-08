using MarcketPlace.Application.Admin.Drivers;
using MarcketPlace.Application.Admin.Drivers.Dtos;
using MarcketPlace.Application.Admin.Vendors;
using MarcketPlace.Application.Admin.Vendors.Dtos;
using MarcketPlace.Application.Users;
using MarcketPlace.Application.Users.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IDriverAdminService _driverAdminService;
        private readonly IVendorAdminService _vendorAdminService;

        public AdminUsersController(
            IUserService userService,
            IDriverAdminService driverAdminService,
            IVendorAdminService vendorAdminService)
        {
            _userService = userService;
            _driverAdminService = driverAdminService;
            _vendorAdminService = vendorAdminService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<UserListItemDto>>> GetAllUsers(CancellationToken cancellationToken)
        {
            var result = await _userService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("drivers")]
        public async Task<ActionResult<IReadOnlyList<DriverListItemDto>>> GetAllDrivers(CancellationToken cancellationToken)
        {
            var result = await _driverAdminService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("drivers/{id:int}")]
        public async Task<ActionResult<DriverDetailsDto>> GetDriverById(int id, CancellationToken cancellationToken)
        {
            var result = await _driverAdminService.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost("drivers")]
        public async Task<ActionResult<DriverDetailsDto>> CreateDriver(
            [FromBody] CreateDriverDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _driverAdminService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetDriverById), new { id = result.Id }, result);
        }

        [HttpPut("drivers/{id:int}")]
        public async Task<ActionResult<DriverDetailsDto>> UpdateDriver(
            int id,
            [FromBody] UpdateDriverDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _driverAdminService.UpdateAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("drivers/{id:int}/status")]
        public async Task<ActionResult<DriverDetailsDto>> UpdateDriverStatus(
            int id,
            [FromBody] UpdateDriverStatusDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _driverAdminService.UpdateStatusAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("vendors")]
        public async Task<ActionResult<IReadOnlyList<VendorAdminListItemDto>>> GetAllVendors(CancellationToken cancellationToken)
        {
            var result = await _vendorAdminService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("vendors/{id:int}")]
        public async Task<ActionResult<VendorAdminDetailsDto>> GetVendorById(int id, CancellationToken cancellationToken)
        {
            var result = await _vendorAdminService.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost("vendors")]
        public async Task<ActionResult<VendorAdminDetailsDto>> CreateVendor(
            [FromBody] CreateVendorByAdminDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _vendorAdminService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetVendorById), new { id = result.VendorId }, result);
        }

        [HttpPut("vendors/{id:int}")]
        public async Task<ActionResult<VendorAdminDetailsDto>> UpdateVendor(
        int id,
        [FromBody] UpdateVendorByAdminDto dto,
        CancellationToken cancellationToken)
        {
            var result = await _vendorAdminService.UpdateAsync(id, dto, cancellationToken);
            return Ok(result);
        }
    }
}