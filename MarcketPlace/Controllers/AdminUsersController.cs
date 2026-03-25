using MarcketPlace.Application.Admin.Customers;
using MarcketPlace.Application.Admin.Customers.Dtos;
using MarcketPlace.Application.Admin.Drivers;
using MarcketPlace.Application.Admin.Drivers.Dtos;
using MarcketPlace.Application.Admin.Vendors;
using MarcketPlace.Application.Admin.Vendors.Dtos;
using MarcketPlace.Application.Users;
using MarcketPlace.Application.Users.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = nameof(UserRole.SuperAdmin))]
    public class AdminUsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IDriverAdminService _driverAdminService;
        private readonly ICustomerAdminService _customerAdminService;
        private readonly IVendorAdminService _vendorAdminService;

        public AdminUsersController(
            IUserService userService,
            IDriverAdminService driverAdminService,
            ICustomerAdminService customerAdminService,
            IVendorAdminService vendorAdminService)
        {
            _userService = userService;
            _driverAdminService = driverAdminService;
            _customerAdminService = customerAdminService;
            _vendorAdminService = vendorAdminService;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<UserListItemDto>>> GetAllUsers(CancellationToken cancellationToken)
        {
            var result = await _userService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserListItemDto>> GetUserById(int id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetByIdAsync(id, cancellationToken);

            if (user is null)
                return NotFound(new { message = $"User with id {id} was not found." });

            return Ok(user);
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

        [HttpGet("customers")]
        public async Task<ActionResult<IReadOnlyList<CustomerListItemDto>>> GetAllCustomers(CancellationToken cancellationToken)
        {
            var result = await _customerAdminService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("customers/{id:int}")]
        public async Task<ActionResult<CustomerDetailsDto>> GetCustomerById(int id, CancellationToken cancellationToken)
        {
            var result = await _customerAdminService.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPost("customers")]
        public async Task<ActionResult<CustomerDetailsDto>> CreateCustomer(
            [FromBody] CreateCustomerDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _customerAdminService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetCustomerById), new { id = result.Id }, result);
        }

        [HttpPut("customers/{id:int}")]
        public async Task<ActionResult<CustomerDetailsDto>> UpdateCustomer(
            int id,
            [FromBody] UpdateCustomerDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _customerAdminService.UpdateAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("customers/{id:int}/status")]
        public async Task<ActionResult<CustomerDetailsDto>> UpdateCustomerStatus(
            int id,
            [FromBody] UpdateCustomerStatusDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _customerAdminService.UpdateStatusAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("vendors")]
        public async Task<ActionResult<IReadOnlyList<VendorAdminListItemDto>>> GetAllVendors(CancellationToken cancellationToken)
        {
            var result = await _vendorAdminService.GetAllAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet("vendors/{id:int}")]
        public async Task<ActionResult<VendorAdminListItemDto>> GetVendorById(int id, CancellationToken cancellationToken)
        {
            var result = await _vendorAdminService.GetByIdAsync(id, cancellationToken);
            if (result is null) return NotFound();
            return Ok(result);
        }

        [HttpPost("vendors")]
        public async Task<ActionResult<VendorAdminListItemDto>> CreateVendor(
            [FromBody] CreateVendorDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _vendorAdminService.CreateAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetVendorById), new { id = result.VendorId }, result);
        }

        [HttpPut("vendors/{id:int}")]
        public async Task<ActionResult<VendorAdminListItemDto>> UpdateVendor(
            int id,
            [FromBody] UpdateVendorDto dto,
            CancellationToken cancellationToken)
        {
            var result = await _vendorAdminService.UpdateAsync(id, dto, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id:int}/change-password")]
        public async Task<IActionResult> ChangeUserPassword(
            int id,
            [FromBody] AdminChangeUserPasswordDto dto,
            CancellationToken cancellationToken)
        {
            await _userService.ChangePasswordByAdminAsync(id, dto, cancellationToken);
            return Ok(new { message = "تم تغيير كلمة السر بنجاح." });
        }
    }
}