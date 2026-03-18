namespace MarcketPlace.Application.Shared.Orders.Dtos
{
    public class OrderPickupDto
    {
        public string PickupLocationNameAr { get; set; } = default!;
        public string PickupLocationNameEn { get; set; } = default!;
        public string PickupAddressText { get; set; } = default!;
        public decimal PickupLatitude { get; set; }
        public decimal PickupLongitude { get; set; }
    }
}