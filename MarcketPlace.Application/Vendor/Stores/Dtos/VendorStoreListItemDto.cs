namespace MarcketPlace.Application.Vendor.Stores.Dtos
{
    public class VendorStoreListItemDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;

        public bool HasLogo { get; set; }
        public string? LogoBase64 { get; set; }
    }
}