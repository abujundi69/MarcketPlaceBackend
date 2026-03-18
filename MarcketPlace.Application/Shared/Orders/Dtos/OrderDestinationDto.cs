namespace MarcketPlace.Application.Shared.Orders.Dtos
{
    public class OrderDestinationDto
    {
        public int DeliveryZoneId { get; set; }
        public string DeliveryZoneNameAr { get; set; } = default!;
        public string DeliveryZoneNameEn { get; set; } = default!;
        public string AddressText { get; set; } = default!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}