namespace MarcketPlace.Application.Customer.Orders.Dtos
{
    public class CreateCustomerOrderFromCartDto
    {
        public int DeliveryZoneId { get; set; }
        public string AddressText { get; set; } = default!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? CustomerNote { get; set; }
    }
}