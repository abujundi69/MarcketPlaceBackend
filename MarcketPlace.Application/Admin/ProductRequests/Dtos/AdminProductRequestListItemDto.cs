using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Admin.ProductRequests.Dtos
{
    public class AdminProductRequestListItemDto
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

        public ProductApprovalStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
    }
}