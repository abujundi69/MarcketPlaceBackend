using MarcketPlace.Application.Admin.Vendors.Dtos;

namespace MarcketPlace.Application.Admin.Vendors
{
    public interface IVendorAdminService
    {
        Task<IReadOnlyList<VendorAdminListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<VendorAdminListItemDto?> GetByIdAsync(int vendorId, CancellationToken cancellationToken = default);
        Task<VendorAdminListItemDto> CreateAsync(CreateVendorDto dto, CancellationToken cancellationToken = default);
        Task<VendorAdminListItemDto> UpdateAsync(int vendorId, UpdateVendorDto dto, CancellationToken cancellationToken = default);
    }
}
