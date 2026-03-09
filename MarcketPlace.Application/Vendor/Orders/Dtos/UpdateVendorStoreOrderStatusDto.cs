using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Vendor.Orders.Dtos
{
    public class UpdateVendorStoreOrderStatusDto
    {
        public OrderStoreStatus Status { get; set; }
        public string? Note { get; set; }
    }
}