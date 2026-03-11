using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Driver.Orders.Dtos
{
    public class DriverOrderDetailsDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = default!;
        public OrderStatus Status { get; set; }

        public string CustomerFullName { get; set; } = default!;
        public string CustomerPhoneNumber { get; set; } = default!;

        public string AddressText { get; set; } = default!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public string DeliveryZoneNameAr { get; set; } = default!;
        public string DeliveryZoneNameEn { get; set; } = default!;

        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}