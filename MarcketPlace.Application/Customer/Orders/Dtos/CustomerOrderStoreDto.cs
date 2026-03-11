using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Customer.Orders.Dtos
{
    public class CustomerOrderStoreDto
    {
        public int OrderStoreId { get; set; }
        public int StoreId { get; set; }

        public string StoreNameAr { get; set; } = default!;
        public string StoreNameEn { get; set; } = default!;

        public OrderStoreStatus Status { get; set; }
        public DateTime? ReadyAt { get; set; }
        public decimal StoreSubtotal { get; set; }

        public List<CustomerOrderItemDto> Items { get; set; } = new();
    }
}