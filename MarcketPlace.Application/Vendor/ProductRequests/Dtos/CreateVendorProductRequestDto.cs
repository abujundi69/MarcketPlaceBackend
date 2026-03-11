namespace MarcketPlace.Application.Vendor.ProductRequests.Dtos
{
    public class CreateVendorProductRequestDto
    {
        public int StoreId { get; set; }
        public int CategoryId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public byte[]? Image { get; set; }

        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinStockQuantity { get; set; }
    }
}