using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Domain.Entities
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? UnitId { get; set; }

        public string? NameAr { get; set; }
        public string? NameEn { get; set; }

        public string? SKU { get; set; }
        public string? Barcode { get; set; }

        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? CostPrice { get; set; }

        public decimal StockQuantity { get; set; }
        public decimal MinStockQuantity { get; set; } = 0m;

        public decimal? MinPurchaseQuantity { get; set; }
        public decimal? MaxPurchaseQuantity { get; set; }
        public decimal? QuantityStep { get; set; }

        public ProductPurchaseInputMode? PurchaseInputModeOverride { get; set; }
        public bool? AllowDecimalQuantityOverride { get; set; }

        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Product Product { get; set; } = default!;
        public ProductUnit? Unit { get; set; }

        public ICollection<ProductVariantOptionValue> VariantOptionValues { get; set; } = new List<ProductVariantOptionValue>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}