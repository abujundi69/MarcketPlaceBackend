using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Admin.Products.Dtos
{
    public class CreateAdminProductDto
    {
        public int CategoryId { get; set; }
        public int? UnitId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }


        public string? ImageBase64 { get; set; }

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

        public List<CreateAdminProductOptionDto> Options { get; set; } = new();
        public List<CreateAdminProductVariantDto> Variants { get; set; } = new();
    }
}