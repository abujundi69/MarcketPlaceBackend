using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Customer.Catalog.Dtos
{
    public class CustomerProductDetailsDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        public byte[]? Image { get; set; }

        public ProductType ProductType { get; set; }
        public ProductPurchaseInputMode PurchaseInputMode { get; set; }
        public bool AllowDecimalQuantity { get; set; }

        public int? UnitId { get; set; }
        public string? UnitNameAr { get; set; }
        public string? UnitNameEn { get; set; }
        public string? UnitSymbol { get; set; }

        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal EffectivePrice { get; set; }

        public decimal StockQuantity { get; set; }
        public decimal MinStockQuantity { get; set; }

        public decimal MinPurchaseQuantity { get; set; }
        public decimal? MaxPurchaseQuantity { get; set; }
        public decimal QuantityStep { get; set; }

        public int? DefaultVariantId { get; set; }

        public List<CustomerProductOptionDto> Options { get; set; } = new();
        public List<CustomerProductVariantDto> Variants { get; set; } = new();
    }
}