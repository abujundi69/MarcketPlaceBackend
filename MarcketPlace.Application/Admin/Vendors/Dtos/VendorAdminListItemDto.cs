namespace MarcketPlace.Application.Admin.Vendors.Dtos
{
    public class VendorAdminListItemDto
    {
        public int VendorId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtText { get; set; } = default!;
        public int StoresCount { get; set; }
        public int? StoreId { get; set; }
        public string? StoreNameAr { get; set; }
        public string? StoreNameEn { get; set; }
        public double StoreAverageRating { get; set; }
        public int StoreRatingsCount { get; set; }
    }
}
