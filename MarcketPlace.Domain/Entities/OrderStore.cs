using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Domain.Entities
{
    public class OrderStore
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int StoreId { get; set; }

        public OrderStoreStatus Status { get; set; }
        public string? Note { get; set; }
        public DateTime? ReadyAt { get; set; }
        public decimal StoreSubtotal { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Order Order { get; set; } = default!;
        public Store Store { get; set; } = default!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}