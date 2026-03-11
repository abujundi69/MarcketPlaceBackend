namespace MarcketPlace.Application.Customer.Cart.Dtos
{
    public class CustomerCartStoreDto
    {
        public int StoreId { get; set; }
        public string StoreNameAr { get; set; } = default!;
        public string StoreNameEn { get; set; } = default!;
        public decimal StoreSubtotal { get; set; }

        public List<CustomerCartItemDto> Items { get; set; } = new();
    }
}