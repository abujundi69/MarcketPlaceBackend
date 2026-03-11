namespace MarcketPlace.Application.Customer.Stores.Dtos
{
    public class CustomerStoreProductDto
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public int CategoryId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public byte[]? Image { get; set; }

        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public string CategoryNameAr { get; set; } = default!;
        public string CategoryNameEn { get; set; } = default!;
    }
}