using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Customer.Cart.Dtos
{
    public class CustomerCartItemDto
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }

        public string ProductNameAr { get; set; } = default!;
        public string ProductNameEn { get; set; } = default!;
        public byte[]? ProductImage { get; set; }

        public string? VariantNameAr { get; set; }
        public string? VariantNameEn { get; set; }

        public string? UnitSymbol { get; set; }

        public ProductPurchaseInputMode PurchaseEntryMode { get; set; }

        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal? RequestedAmount { get; set; }
        public decimal LineTotal { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}