using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Customer.Orders.Dtos
{
    public class CustomerCreatedOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = default!;
        public OrderStatus Status { get; set; }

        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}