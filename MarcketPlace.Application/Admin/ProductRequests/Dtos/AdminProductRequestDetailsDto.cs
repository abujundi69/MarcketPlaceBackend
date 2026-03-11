using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Admin.ProductRequests.Dtos
{
    public class AdminProductRequestDetailsDto
    {
        public int Id { get; set; }

        public int VendorId { get; set; }
        public string VendorOwnerName { get; set; } = default!;

        public int StoreId { get; set; }
        public string StoreNameAr { get; set; } = default!;
        public string StoreNameEn { get; set; } = default!;

        public int CategoryId { get; set; }
        public string CategoryNameAr { get; set; } = default!;
        public string CategoryNameEn { get; set; } = default!;

        public string ProductNameAr { get; set; } = default!;
        public string ProductNameEn { get; set; } = default!;
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
    }
}