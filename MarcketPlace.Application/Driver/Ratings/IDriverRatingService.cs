using MarcketPlace.Application.Driver.Ratings.Dtos;

namespace MarcketPlace.Application.Driver.Ratings
{
    public interface IDriverRatingService
    {
        Task<DriverRatingsDto> GetMyRatingsAsync(
            int driverUserId,
            CancellationToken cancellationToken = default);
    }
}
