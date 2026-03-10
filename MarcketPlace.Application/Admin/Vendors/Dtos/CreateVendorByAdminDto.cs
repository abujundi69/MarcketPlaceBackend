using System.ComponentModel.DataAnnotations;

namespace MarcketPlace.Application.Admin.Vendors.Dtos
{
    public class CreateVendorByAdminDto
    {
        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = default!;

        [Required]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = default!;

        public bool IsActive { get; set; } = true;

        public bool IsApproved { get; set; } = true;
    }
}