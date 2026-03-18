using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; } = default!;

        public int CustomerId { get; set; }
        public int? DriverId { get; set; }
        public int DeliveryZoneId { get; set; }

        public OrderStatus Status { get; set; }

        public string AddressText { get; set; } = default!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public string PickupLocationNameAr { get; set; } = default!;
        public string PickupLocationNameEn { get; set; } = default!;
        public string PickupAddressText { get; set; } = default!;
        public decimal PickupLatitude { get; set; }
        public decimal PickupLongitude { get; set; }

        public string? CustomerNote { get; set; }
        public string? CustomerWhatsAppNumber { get; set; }

        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal TotalAmount { get; set; }

        public DateTime? DriverAssignedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public string? CancelReason { get; set; }
        public int? CancelledByUserId { get; set; }
        public DateTime? CancelledAt { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Customer Customer { get; set; } = default!;
        public Driver? Driver { get; set; }
        public DeliveryZone DeliveryZone { get; set; } = default!;
        public User? CancelledByUser { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<DriverRating> DriverRatings { get; set; } = new List<DriverRating>();
    }
}