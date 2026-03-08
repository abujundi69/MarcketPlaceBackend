namespace MarcketPlace.Application.Admin.Stores.Dtos
{
    public class StoreAdminListItemDto
    {
        public int Id { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;

        public string PhoneNumber { get; set; } = default!;
        public string AddressText { get; set; } = default!;

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public bool IsActive { get; set; }

        public int? VendorId { get; set; }
        public string? VendorName { get; set; }
        public string? VendorPhoneNumber { get; set; }
        public bool? VendorIsApproved { get; set; }

        public decimal AverageRating { get; set; }
        public int RatingsCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedAtText { get; set; } = default!;
    }
}