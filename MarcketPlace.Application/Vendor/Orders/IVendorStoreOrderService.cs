using MarcketPlace.Application.Vendor.Orders.Dtos;

namespace MarcketPlace.Application.Vendor.Orders
{
    public interface IVendorStoreOrderService
    {
        Task<IReadOnlyList<VendorStoreOrderListItemDto>> GetStoreOrdersAsync(
            int vendorUserId,
            int storeId,
            CancellationToken cancellationToken = default);

        Task<VendorStoreOrderDetailsDto> GetStoreOrderDetailsAsync(
            int vendorUserId,
            int storeId,
            int orderStoreId,
            CancellationToken cancellationToken = default);
        Task<VendorStoreOrderDetailsDto> UpdateStoreOrderStatusAsync(
            int vendorUserId,
            int storeId,
            int orderStoreId,
            UpdateVendorStoreOrderStatusDto dto,
            CancellationToken cancellationToken = default);
    }
}