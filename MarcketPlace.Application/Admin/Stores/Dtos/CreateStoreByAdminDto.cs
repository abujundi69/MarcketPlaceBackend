using System.ComponentModel.DataAnnotations;

namespace MarcketPlace.Application.Admin.Stores.Dtos
{
    public class CreateStoreByAdminDto
    {
        [Required]
        [StringLength(200)]
        public string NameAr { get; set; } = default!;

        [Required]
        [StringLength(200)]
        public string NameEn { get; set; } = default!;

        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }

        [Required]
        [StringLength(30)]
        public string PhoneNumber { get; set; } = default!;

        [Required]
        [StringLength(500)]
        public string AddressText { get; set; } = default!;

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(1, int.MaxValue)]
        public int? VendorId { get; set; }

        public string? LogoBase64 { get; set; }
    }
}