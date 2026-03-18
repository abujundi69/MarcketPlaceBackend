using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Vendor.ProductRequests.Dtos
{
    public class VendorProductRequestDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? UnitId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        public byte[]? Image { get; set; }

        public ProductApprovalStatus Status { get; set; }
        public string? AdminNote { get; set; }

        public string CategoryNameAr { get; set; } = default!;
        public string CategoryNameEn { get; set; } = default!;
        public string? UnitNameAr { get; set; }
        public string? UnitNameEn { get; set; }

        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public int? ProductId { get; set; }
    }
}
