namespace MarcketPlace.Application.Admin.Stores.Dtos
{
    public class StoreAdminDetailsDto
    {
        public int Id { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;

        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        public string PhoneNumber { get; set; } = default!;
        public string AddressText { get; set; } = default!;

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public bool IsActive { get; set; }

        public int? VendorId { get; set; }
        public string? VendorName { get; set; }
        public string? VendorPhoneNumber { get; set; }
        public bool? VendorIsApproved { get; set; }

        public string? LogoBase64 { get; set; }
        public bool HasLogo { get; set; }

        public decimal AverageRating { get; set; }
        public int RatingsCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedAtText { get; set; } = default!;

        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedAtText { get; set; }
    }
}