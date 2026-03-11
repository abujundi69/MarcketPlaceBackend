using MarcketPlace.Application.Customer.Stores;
using MarcketPlace.Application.Customer.Stores.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    [ApiController]
    [Route("api/customer/stores")]
    public class CustomerStoreCatalogController : ControllerBase
    {
        private readonly ICustomerStoreCatalogService _customerStoreCatalogService;

        public CustomerStoreCatalogController(ICustomerStoreCatalogService customerStoreCatalogService)
        {
            _customerStoreCatalogService = customerStoreCatalogService;
        }

        [HttpGet("{storeId:int}/products")]
        public async Task<ActionResult<IReadOnlyList<CustomerStoreProductDto>>> GetStoreProducts(
            [FromRoute] int storeId,
            [FromQuery] int? categoryId,
            CancellationToken cancellationToken)
        {
            var result = await _customerStoreCatalogService.GetStoreProductsAsync(
                storeId,
                categoryId,
                cancellationToken);

            return Ok(result);
        }

    }

}