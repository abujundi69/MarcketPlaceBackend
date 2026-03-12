namespace MarcketPlace.Application.Vendor.Products.Dtos
{
    public class UpdateVendorProductDto
    {
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public byte[]? Image { get; set; }
    }
}