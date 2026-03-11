using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Customer.Orders.Dtos
{
    public class CustomerOrderListItemDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = default!;

        public OrderStatus Status { get; set; }

        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? DriverAssignedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public CustomerOrderDriverDto? Driver { get; set; }
    }
}