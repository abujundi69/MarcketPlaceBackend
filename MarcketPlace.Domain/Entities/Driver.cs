namespace MarcketPlace.Domain.Entities
{
    public class Driver
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string VehicleType { get; set; } = default!;
        public string VehicleNumber { get; set; } = default!;
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = default!;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<DriverRating> DriverRatings { get; set; } = new List<DriverRating>();
    }
}