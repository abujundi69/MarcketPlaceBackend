namespace MarcketPlace.Application.Driver.Orders.Dtos
{
    public class DriverAvailableOrderStoreDto
    {
        public int OrderStoreId { get; set; }
        public int StoreId { get; set; }

        public string StoreNameAr { get; set; } = default!;
        public string StoreNameEn { get; set; } = default!;

        public decimal StoreSubtotal { get; set; }
    }
}