using MarcketPlace.Application.Admin.Vendors.Dtos;

namespace MarcketPlace.Application.Admin.Vendors
{
    public interface IVendorAdminService
    {
        Task<VendorAdminDetailsDto> CreateAsync(CreateVendorByAdminDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<VendorAdminListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<VendorAdminDetailsDto?> GetByIdAsync(int vendorId, CancellationToken cancellationToken = default);
        Task<VendorAdminDetailsDto> UpdateAsync(int vendorId, UpdateVendorByAdminDto dto, CancellationToken cancellationToken = default);

    }
}