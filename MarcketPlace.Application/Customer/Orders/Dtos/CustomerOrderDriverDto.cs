namespace MarcketPlace.Application.Customer.Orders.Dtos
{
    public class CustomerOrderDriverDto
    {
        public int DriverId { get; set; }
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string VehicleType { get; set; } = default!;
        public string VehicleNumber { get; set; } = default!;
    }
}