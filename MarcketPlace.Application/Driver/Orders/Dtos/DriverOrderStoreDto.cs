using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Driver.Orders.Dtos
{
    public class DriverOrderStoreDto
    {
        public int OrderStoreId { get; set; }
        public int StoreId { get; set; }

        public string StoreNameAr { get; set; } = default!;
        public string StoreNameEn { get; set; } = default!;

        public decimal StoreSubtotal { get; set; }
        public OrderStoreStatus Status { get; set; }
        public DateTime? ReadyAt { get; set; }
    }
}