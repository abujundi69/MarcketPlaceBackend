namespace MarcketPlace.Application.Admin.Vendors.Dtos
{
    public class UpdateVendorDto
    {
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
    }
}
