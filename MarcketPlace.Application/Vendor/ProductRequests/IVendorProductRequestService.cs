using MarcketPlace.Application.Vendor.ProductRequests.Dtos;

namespace MarcketPlace.Application.Vendor.ProductRequests
{
    public interface IVendorProductRequestService
    {
        Task<VendorProductRequestDto> CreateAsync(
            int vendorUserId,
            CreateVendorProductRequestDto dto,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<VendorProductRequestDto>> GetMineAsync(
            int vendorUserId,
            CancellationToken cancellationToken = default);

        Task<VendorProductRequestDto?> GetByIdAsync(
            int vendorUserId,
            int productRequestId,
            CancellationToken cancellationToken = default);
    }
}
