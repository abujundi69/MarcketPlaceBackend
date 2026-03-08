namespace MarcketPlace.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderStoreId { get; set; }
        public int ProductId { get; set; }

        public string ProductNameAr { get; set; } = default!;
        public string ProductNameEn { get; set; } = default!;
        public string? ProductImageUrl { get; set; }

        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }

        public DateTime CreatedAt { get; set; }

        public OrderStore OrderStore { get; set; } = default!;
        public Product Product { get; set; } = default!;
    }
}