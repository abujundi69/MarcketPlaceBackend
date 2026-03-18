namespace MarcketPlace.Application.Admin.Vendors.Dtos
{
    public class CreateVendorDto
    {
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; } = true;
    }
}
