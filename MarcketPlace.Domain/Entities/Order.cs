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
        public string? CustomerNote { get; set; }

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

        public ICollection<OrderStore> OrderStores { get; set; } = new List<OrderStore>();
        public ICollection<StoreRating> StoreRatings { get; set; } = new List<StoreRating>();
        public ICollection<DriverRating> DriverRatings { get; set; } = new List<DriverRating>();
    }
}