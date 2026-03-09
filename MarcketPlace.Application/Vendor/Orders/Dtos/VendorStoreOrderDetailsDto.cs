using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Vendor.Orders.Dtos
{
    public class VendorStoreOrderDetailsDto
    {
        public int OrderStoreId { get; set; }
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = default!;

        public int StoreId { get; set; }
        public string StoreNameAr { get; set; } = default!;
        public string StoreNameEn { get; set; } = default!;

        public OrderStatus OrderStatus { get; set; }
        public OrderStoreStatus StoreOrderStatus { get; set; }

        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = default!;
        public string CustomerPhoneNumber { get; set; } = default!;

        public string AddressText { get; set; } = default!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? CustomerNote { get; set; }

        public decimal StoreSubtotal { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DriverAssignedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public List<VendorStoreOrderItemDto> Items { get; set; } = new();
    }
}