using MarcketPlace.Application.Vendor.ProductRequests.Dtos;

namespace MarcketPlace.Application.Vendor.ProductRequests
{
    public interface IVendorProductRequestService
    {
        Task<VendorProductRequestDto> CreateAsync(
            int vendorId,
            CreateVendorProductRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<VendorProductRequestDto> CreateByUserAsync(
            int userId,
            CreateVendorProductRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<VendorProductRequestDto>> GetMyRequestsAsync(
            int vendorId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<VendorProductRequestDto>> GetMyRequestsByUserAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}