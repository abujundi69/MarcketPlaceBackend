namespace MarcketPlace.Application.Customer.Ratings.Dtos
{
    public class CustomerDriverRatingDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int DriverId { get; set; }
        public int CustomerId { get; set; }
        public int Score { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}