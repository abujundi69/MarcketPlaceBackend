namespace MarcketPlace.Application.Customer.Locations.Dtos
{
    public class CustomerSavedLocationDto
    {
        public int? DeliveryZoneId { get; set; }
        public string? DeliveryZoneNameAr { get; set; }
        public string? DeliveryZoneNameEn { get; set; }

        public string? AddressText { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public DateTime? LocationUpdatedAt { get; set; }
    }
}