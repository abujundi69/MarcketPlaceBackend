namespace MarcketPlace.Application.Admin.Drivers.Dtos
{
    public class DriverListItemDto
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
        public string CreatedAtText { get; set; } = default!;

        public double AverageRating { get; set; }
        public int RatingsCount { get; set; }
        public string AverageRatingText { get; set; } = default!;
    }
}