using MarcketPlace.Application.Customer.Locations.Dtos;

namespace MarcketPlace.Application.Customer.Locations
{
    public interface ICustomerLocationService
    {
        Task<CustomerSavedLocationDto> GetMyLocationAsync(
            int customerUserId,
            CancellationToken cancellationToken = default);

        Task<CustomerSavedLocationDto> UpdateMyLocationAsync(
            int customerUserId,
            UpdateCustomerSavedLocationDto dto,
            CancellationToken cancellationToken = default);
    }
}