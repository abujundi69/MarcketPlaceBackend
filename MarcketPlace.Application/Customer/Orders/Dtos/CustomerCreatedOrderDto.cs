using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Customer.Orders.Dtos
{
    public class CustomerCreatedOrderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = default!;

        public OrderStatus Status { get; set; }

        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }

        public string AddressText { get; set; } = default!;
        public string? CustomerNote { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<CustomerCreatedOrderStoreDto> Stores { get; set; } = new();
    }
}