namespace MarcketPlace.Application.Customer.Stores.Dtos
{
    public class StoreDetailsDto
    {
        public int Id { get; set; }
        public int? VendorId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;

        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        public string PhoneNumber { get; set; } = default!;
        public string AddressText { get; set; } = default!;

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public bool IsActive { get; set; }

        public string? LogoBase64 { get; set; }
        public bool HasLogo { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}