namespace MarcketPlace.Application.Customer.Cart.Dtos
{
    public class CustomerCartItemDto
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }

        public string ProductNameAr { get; set; } = default!;
        public string ProductNameEn { get; set; } = default!;
        public byte[]? ProductImage { get; set; }

        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }

        public int AvailableStock { get; set; }
    }
}