namespace MarcketPlace.Application.Admin.DeliveryZones.Dtos
{
    public class UpdateDeliveryZoneDto
    {
        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public decimal DeliveryFee { get; set; }
    }
}