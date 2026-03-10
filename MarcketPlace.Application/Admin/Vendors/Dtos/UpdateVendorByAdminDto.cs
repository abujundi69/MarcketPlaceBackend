using System.ComponentModel.DataAnnotations;

namespace MarcketPlace.Application.Admin.Vendors.Dtos
{
    public class UpdateVendorByAdminDto
    {
        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = default!;

        [Required]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = default!;

        public bool IsActive { get; set; }

        public bool IsApproved { get; set; }

        public int? StoreId { get; set; }
    }
}