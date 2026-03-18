namespace MarcketPlace.Application.Customer.Cart.Dtos
{
    public class CustomerCartDto
    {
        public int ItemsCount { get; set; }
        public decimal Subtotal { get; set; }
        public List<CustomerCartItemDto> Items { get; set; } = new();
    }
}