namespace MarcketPlace.Application.Customer.Locations.Dtos
{
    public class UpdateCustomerSavedLocationDto
    {
        public int DeliveryZoneId { get; set; }
        public string AddressText { get; set; } = default!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}