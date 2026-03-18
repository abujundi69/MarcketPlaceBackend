using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Domain.Entities
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public int ProductId { get; set; }

        public int? ProductVariantId { get; set; }

        public decimal Quantity { get; set; }

        public decimal? RequestedAmount { get; set; }

        public ProductPurchaseInputMode PurchaseEntryMode { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineTotal { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Customer Customer { get; set; } = default!;
        public Product Product { get; set; } = default!;
        public ProductVariant? ProductVariant { get; set; }
    }
}