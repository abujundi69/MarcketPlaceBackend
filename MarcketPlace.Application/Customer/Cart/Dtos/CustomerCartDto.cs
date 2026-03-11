namespace MarcketPlace.Application.Customer.Cart.Dtos
{
    public class CustomerCartDto
    {
        public int TotalItemsCount { get; set; }
        public int StoresCount { get; set; }
        public decimal Subtotal { get; set; }

        public List<CustomerCartStoreDto> Stores { get; set; } = new();
    }
}