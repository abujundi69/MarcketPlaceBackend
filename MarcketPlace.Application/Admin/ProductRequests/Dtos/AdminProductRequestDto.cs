using System.Text.Json.Serialization;
using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Admin.ProductRequests.Dtos
{
    public class AdminProductRequestDto
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int StoreId { get; set; }
        public int CategoryId { get; set; }
        public int? UnitId { get; set; }

        [JsonPropertyName("productNameAr")]
        public string NameAr { get; set; } = default!;

        [JsonPropertyName("productNameEn")]
        public string NameEn { get; set; } = default!;

        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        [JsonIgnore]
        public byte[]? Image { get; set; }

        public string? ImageUrl => Image != null ? "data:image/jpeg;base64," + Convert.ToBase64String(Image) : null;

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

        public ProductApprovalStatus Status { get; set; }
        public string? AdminNote { get; set; }

        [JsonPropertyName("vendorOwnerName")]
        public string VendorName { get; set; } = default!;

        public string StoreNameAr { get; set; } = default!;
        public string StoreNameEn { get; set; } = default!;
        public string CategoryNameAr { get; set; } = default!;
        public string CategoryNameEn { get; set; } = default!;
        public string? UnitNameAr { get; set; }

        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedByUserId { get; set; }
        public int? ProductId { get; set; }
    }
}
