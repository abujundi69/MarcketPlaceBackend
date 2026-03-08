namespace MarcketPlace.Application.Admin.Drivers.Dtos
{
    public class DriverDetailsDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;

        public string VehicleType { get; set; } = default!;
        public string VehicleNumber { get; set; } = default!;

        public bool IsActive { get; set; }
        public string StatusText { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string CreatedAtText { get; set; } = default!;
        public string? UpdatedAtText { get; set; }
    }
}