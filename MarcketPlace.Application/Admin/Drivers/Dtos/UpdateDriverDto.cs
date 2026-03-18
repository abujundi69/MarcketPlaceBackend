namespace MarcketPlace.Application.Admin.Drivers.Dtos
{
    public class UpdateDriverDto
    {
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string VehicleType { get; set; } = default!;
        public string VehicleNumber { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
