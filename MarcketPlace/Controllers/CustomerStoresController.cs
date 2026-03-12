using MarcketPlace.Application.Customer.Stores;
using MarcketPlace.Application.Customer.Stores.Dtos;
using MarcketPlace.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/customer/stores")]
    [Authorize(Roles = nameof(UserRole.Customer))]
    public class CustomerStoresController : ControllerBase
    {
        private readonly ICustomerStoreCatalogService _customerStoreService;

        public CustomerStoresController(ICustomerStoreCatalogService customerStoreService)
        {
            _customerStoreService = customerStoreService;
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IReadOnlyList<StoreCategoryDto>>> GetCategories(CancellationToken cancellationToken)
        {
            var result = await _customerStoreService.GetStoreCategoriesAsync(cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<StoreListItemDto>>> GetAll(
            [FromQuery] int? categoryId,
            CancellationToken cancellationToken)
        {
            var result = await _customerStoreService.GetAllAsync(categoryId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<StoreDetailsDto>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _customerStoreService.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }
    }
}