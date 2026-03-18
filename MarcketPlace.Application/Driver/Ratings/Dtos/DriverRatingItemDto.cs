namespace MarcketPlace.Application.Driver.Ratings.Dtos
{
    public class DriverRatingItemDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public int Score { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
