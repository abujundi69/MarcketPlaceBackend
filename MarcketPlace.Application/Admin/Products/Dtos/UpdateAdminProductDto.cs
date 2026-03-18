using System.Text.Json.Serialization;

namespace MarcketPlace.Application.Admin.Products.Dtos
{
    public class UpdateAdminProductDto
    {
        public int CategoryId { get; set; }
        public int? UnitId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        public string? ImageBase64 { get; set; }

        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? CostPrice { get; set; }

        public decimal StockQuantity { get; set; }
        public decimal MinStockQuantity { get; set; }

        public decimal MinPurchaseQuantity { get; set; }
        public decimal? MaxPurchaseQuantity { get; set; }
        public decimal QuantityStep { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }
    }
}
