using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Domain.Entities
{
    public class ProductRequest
    {
        public int Id { get; set; }

        public int VendorId { get; set; }
        public int StoreId { get; set; }
        public int CategoryId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public byte[]? Image { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinStockQuantity { get; set; }

        public ProductApprovalStatus Status { get; set; }
        public string? AdminNote { get; set; }

        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedByUserId { get; set; }

        public int? ProductId { get; set; }

        public Vendor Vendor { get; set; } = default!;
        public Store Store { get; set; } = default!;
        public Category Category { get; set; } = default!;
        public Product? Product { get; set; }
    }
}