using System.Text.Json.Serialization;
using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Admin.Products.Dtos
{
    public class AdminProductDto
    {
        public int Id { get; set; }

        /// <summary>متوافق مع الفرونت إند - المنتجات من الأدمن بدون متجر.</summary>
        public int? StoreId { get; set; }

        public int CategoryId { get; set; }
        public int? UnitId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        public byte[]? Image { get; set; }

        public ProductType ProductType { get; set; }
        public ProductPurchaseInputMode PurchaseInputMode { get; set; }
        public bool AllowDecimalQuantity { get; set; }

        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? CostPrice { get; set; }

        public decimal StockQuantity { get; set; }
        public decimal MinStockQuantity { get; set; }

        public decimal MinPurchaseQuantity { get; set; }
        public decimal? MaxPurchaseQuantity { get; set; }
        public decimal QuantityStep { get; set; }

        public bool IsActive { get; set; }

        /// <summary>متوافق مع الفرونت إند: 2=معتمد، 3=معطل.</summary>
        [JsonPropertyName("approvalStatus")]
        public int ApprovalStatus => IsActive ? 2 : 3;

        public string CategoryNameAr { get; set; } = default!;
        public string CategoryNameEn { get; set; } = default!;

        public string? UnitNameAr { get; set; }
        public string? UnitNameEn { get; set; }
        public string? UnitSymbol { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<AdminProductOptionDto> Options { get; set; } = new();
        public List<AdminProductVariantDto> Variants { get; set; } = new();
    }
}