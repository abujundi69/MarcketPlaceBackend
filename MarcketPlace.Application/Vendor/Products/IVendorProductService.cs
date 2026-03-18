using MarcketPlace.Application.Vendor.Products.Dtos;

namespace MarcketPlace.Application.Vendor.Products
{
    public interface IVendorProductService
    {
        Task<IReadOnlyList<VendorProductDto>> GetMyProductsAsync(int vendorUserId, CancellationToken cancellationToken = default);
        Task<VendorProductDto?> GetByIdAsync(int vendorUserId, int productId, CancellationToken cancellationToken = default);
    }
}
