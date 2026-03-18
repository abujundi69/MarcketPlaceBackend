using MarcketPlace.Application.Admin.Products;
using MarcketPlace.Application.Admin.Products.Dtos;
using MarcketPlace.Application.Admin.ProductsDoscount.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/product-discounts")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminProductDiscountsController : ControllerBase
    {
        private readonly IAdminProductDiscountService _discountService;

        public AdminProductDiscountsController(IAdminProductDiscountService discountService)
        {
            _discountService = discountService;
        }

        [HttpPost("product/{productId:int}")]
        public async Task<IActionResult> SetProductDiscount(
            int productId,
            [FromBody] SetProductDiscountDto dto,
            CancellationToken cancellationToken)
        {
            await _discountService.SetProductDiscountAsync(productId, dto, cancellationToken);
            return Ok(new { message = "تم تطبيق الخصم على المنتج بنجاح." });
        }

        [HttpDelete("product/{productId:int}")]
        public async Task<IActionResult> ClearProductDiscount(
            int productId,
            CancellationToken cancellationToken)
        {
            await _discountService.ClearProductDiscountAsync(productId, cancellationToken);
            return Ok(new { message = "تمت إزالة الخصم عن المنتج بنجاح." });
        }

        [HttpPost("variant/{variantId:int}")]
        public async Task<IActionResult> SetVariantDiscount(
            int variantId,
            [FromBody] SetVariantDiscountDto dto,
            CancellationToken cancellationToken)
        {
            await _discountService.SetVariantDiscountAsync(variantId, dto, cancellationToken);
            return Ok(new { message = "تم تطبيق الخصم على الـ Variant بنجاح." });
        }

        [HttpDelete("variant/{variantId:int}")]
        public async Task<IActionResult> ClearVariantDiscount(
            int variantId,
            CancellationToken cancellationToken)
        {
            await _discountService.ClearVariantDiscountAsync(variantId, cancellationToken);
            return Ok(new { message = "تمت إزالة الخصم عن الـ Variant بنجاح." });
        }
    }
}