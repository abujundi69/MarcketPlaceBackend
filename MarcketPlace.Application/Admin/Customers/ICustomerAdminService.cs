using MarcketPlace.Application.Admin.Customers.Dtos;

namespace MarcketPlace.Application.Admin.Customers
{
    public interface ICustomerAdminService
    {
        Task<IReadOnlyList<CustomerListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<CustomerDetailsDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CustomerDetailsDto> CreateAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default);
        Task<CustomerDetailsDto> UpdateAsync(int id, UpdateCustomerDto dto, CancellationToken cancellationToken = default);
        Task<CustomerDetailsDto> UpdateStatusAsync(int id, UpdateCustomerStatusDto dto, CancellationToken cancellationToken = default);
    }
}