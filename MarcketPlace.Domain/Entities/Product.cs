using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? UnitId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        public byte[]? Image { get; set; }

        public ProductType ProductType { get; set; } = ProductType.Simple;
        public ProductPurchaseInputMode PurchaseInputMode { get; set; } = ProductPurchaseInputMode.QuantityOnly;
        public bool AllowDecimalQuantity { get; set; }

        public decimal Price { get; set; }

        public decimal? SalePrice { get; set; }
        public decimal? CostPrice { get; set; }

        public decimal StockQuantity { get; set; }
        public decimal MinStockQuantity { get; set; }

        public decimal MinPurchaseQuantity { get; set; } = 1m;
        public decimal? MaxPurchaseQuantity { get; set; }
        public decimal QuantityStep { get; set; } = 1m;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Category Category { get; set; } = default!;
        public ProductUnit? Unit { get; set; }

        public ICollection<ProductOption> Options { get; set; } = new List<ProductOption>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<CustomerFavorite> CustomerFavorites { get; set; } = new List<CustomerFavorite>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}