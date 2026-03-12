using MarcketPlace.Application.Admin.DeliveryZones;
using MarcketPlace.Application.Admin.DeliveryZones.Dtos;
using MarcketPlace.Application.Admin.SystemSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarcketPlace.Controllers
{
    /// <summary>
    /// APIs عامة بدون مصادقة — مثل رسالة الترويج ومناطق التوصيل.
    /// </summary>
    [ApiController]
    [Route("api/public")]
    [AllowAnonymous]
    public class PublicController : ControllerBase
    {
        private readonly ISystemSettingAdminService _systemSettings;
        private readonly IDeliveryZoneAdminService _deliveryZones;

        public PublicController(
            ISystemSettingAdminService systemSettings,
            IDeliveryZoneAdminService deliveryZones)
        {
            _systemSettings = systemSettings;
            _deliveryZones = deliveryZones;
        }

        /// <summary>
        /// جلب رسالة الترويج المعروضة للعملاء في التطبيق (يتم تعيينها من لوحة تحكم الأدمن).
        /// </summary>
        [HttpGet("promo-message")]
        public async Task<ActionResult<PromoMessageResponse>> GetPromoMessage(CancellationToken cancellationToken)
        {
            var message = await _systemSettings.GetCustomerPromoMessageAsync(cancellationToken);
            return Ok(new PromoMessageResponse { Message = message });
        }

        /// <summary>
        /// جلب مناطق التوصيل النشطة (لاختيار العميل عند تعيين عنوانه).
        /// </summary>
        [HttpGet("delivery-zones")]
        public async Task<ActionResult<IReadOnlyList<DeliveryZoneDto>>> GetDeliveryZones(CancellationToken cancellationToken)
        {
            var zones = await _deliveryZones.GetAllAsync(cancellationToken);
            return Ok(zones);
        }
    }

    public class PromoMessageResponse
    {
        public string? Message { get; set; }
    }
}
