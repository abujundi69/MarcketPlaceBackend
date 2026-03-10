using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Vendor.ProductRequests.Dtos
{
    public class VendorProductRequestDto
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public int StoreId { get; set; }
        public int CategoryId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinStockQuantity { get; set; }

        public ProductApprovalStatus Status { get; set; }
        public string? AdminNote { get; set; }

        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ProductId { get; set; }
    }
}