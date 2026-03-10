namespace MarcketPlace.Application.Orders.Dtos
{
    public class CheckoutOrderDto
    {
        public int CustomerId { get; set; }
        public int DeliveryZoneId { get; set; }

        public string AddressText { get; set; } = default!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? CustomerNote { get; set; }
    }
}