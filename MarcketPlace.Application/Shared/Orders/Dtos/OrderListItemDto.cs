using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Shared.Orders.Dtos
{
    public class OrderListItemDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = default!;
        public OrderStatus Status { get; set; }

        public string CustomerName { get; set; } = default!;
        public string CustomerPhoneNumber { get; set; } = default!;
        public string? CustomerWhatsAppNumber { get; set; }

        public string? DriverName { get; set; }
        public string? DriverPhoneNumber { get; set; }

        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? DriverAssignedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public OrderPickupDto Pickup { get; set; } = default!;
        public OrderDestinationDto Destination { get; set; } = default!;
    }
}