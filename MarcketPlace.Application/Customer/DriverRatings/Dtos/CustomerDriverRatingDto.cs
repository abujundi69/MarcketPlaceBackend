namespace MarcketPlace.Application.Customer.DriverRatings.Dtos
{
    public class CustomerDriverRatingDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int DriverId { get; set; }

        public string DriverName { get; set; } = default!;
        public string? DriverPhoneNumber { get; set; }

        public int Score { get; set; }
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}