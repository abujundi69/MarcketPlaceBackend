namespace MarcketPlace.Application.Orders.Dtos
{
    public class CheckoutOrderResultDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = default!;
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}