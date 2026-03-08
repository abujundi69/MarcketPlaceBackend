using MarcketPlace.Application.Vendor.ProductRequests.Dtos;

namespace MarcketPlace.Application.Vendor.ProductRequests
{
    public interface IVendorProductRequestService
    {
        Task<VendorProductRequestDto> CreateAsync(int vendorId, CreateVendorProductRequestDto dto, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<VendorProductRequestDto>> GetMyRequestsAsync(int vendorId, CancellationToken cancellationToken = default);
    }
}