using MarcketPlace.Application.Customer.DriverRatings.Dtos;

namespace MarcketPlace.Application.Customer.DriverRatings
{
    public interface ICustomerDriverRatingService
    {
        Task<CustomerDriverRatingDto> RateOrderDriverAsync(
            int customerUserId,
            int orderId,
            CreateCustomerDriverRatingDto dto,
            CancellationToken cancellationToken = default);

        Task<CustomerDriverRatingDto> GetByOrderAsync(
            int customerUserId,
            int orderId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CustomerDriverRatingDto>> GetMyRatingsAsync(
            int customerUserId,
            CancellationToken cancellationToken = default);
    }
}