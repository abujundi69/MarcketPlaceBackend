using MarcketPlace.Application.Customer.Ratings.Dtos;

namespace MarcketPlace.Application.Customer.Ratings
{
    public interface ICustomerRatingService
    {
        Task<CustomerOrderRatingAvailabilityDto> GetOrderRatingAvailabilityAsync(
            int customerUserId,
            int orderId,
            CancellationToken cancellationToken = default);

        Task<CustomerDriverRatingDto> CreateDriverRatingAsync(
            int customerUserId,
            int orderId,
            CreateDriverRatingDto dto,
            CancellationToken cancellationToken = default);

        Task<CustomerStoreRatingDto> CreateStoreRatingAsync(
            int customerUserId,
            int orderId,
            int storeId,
            CreateStoreRatingDto dto,
            CancellationToken cancellationToken = default);
    }
}