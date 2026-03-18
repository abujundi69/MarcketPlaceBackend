namespace MarcketPlace.Application.Driver.Ratings.Dtos
{
    public class DriverRatingsDto
    {
        public double AverageRating { get; set; }
        public int RatingsCount { get; set; }
        public string AverageRatingText { get; set; } = default!;
        public List<DriverRatingItemDto> Ratings { get; set; } = new();
    }
}
