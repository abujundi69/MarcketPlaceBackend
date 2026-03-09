using MarcketPlace.Application.Vendor.Products.Dtos;

namespace MarcketPlace.Application.Vendor.Products
{
    public interface IVendorProductService
    {
        Task<IReadOnlyList<VendorProductDto>> GetAllByUserAsync(
            int userId,
            CancellationToken cancellationToken = default);

        Task<VendorProductDto> GetByIdByUserAsync(
            int userId,
            int productId,
            CancellationToken cancellationToken = default);

        Task<VendorProductDto> UpdateByUserAsync(
            int userId,
            int productId,
            UpdateVendorProductDto dto,
            CancellationToken cancellationToken = default);
    }
}