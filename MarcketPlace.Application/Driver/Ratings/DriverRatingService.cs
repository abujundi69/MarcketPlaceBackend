using MarcketPlace.Application.Driver.Ratings.Dtos;
using MarcketPlace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MarcketPlace.Application.Driver.Ratings
{
    public class DriverRatingService : IDriverRatingService
    {
        private readonly AppDbContext _context;

        public DriverRatingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DriverRatingsDto> GetMyRatingsAsync(
            int driverUserId,
            CancellationToken cancellationToken = default)
        {
            var driver = await _context.Drivers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == driverUserId, cancellationToken);

            if (driver is null)
                throw new KeyNotFoundException("المندوب غير موجود.");

            var ratings = await _context.DriverRatings
                .AsNoTracking()
                .Include(x => x.Order)
                .Where(x => x.DriverId == driver.Id)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            var count = ratings.Count;
            var average = count > 0 ? ratings.Average(x => x.Score) : 0.0;
            var averageText = count > 0 ? $"{average:0.0} / 5" : "0 / 5";

            var items = ratings
                .Select(x => new DriverRatingItemDto
                {
                    Id = x.Id,
                    OrderId = x.OrderId,
                    OrderNumber = x.Order.OrderNumber,
                    Score = x.Score,
                    Comment = x.Comment,
                    CreatedAt = x.CreatedAt
                })
                .ToList();

            return new DriverRatingsDto
            {
                AverageRating = average,
                RatingsCount = count,
                AverageRatingText = averageText,
                Ratings = items
            };
        }
    }
}
