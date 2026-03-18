namespace MarcketPlace.Application.Vendor.Products.Dtos
{
    /// <summary>منتج التاجر — من طلبات معتمدة.</summary>
    public class VendorProductDto
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public int CategoryId { get; set; }
        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public string? Image { get; set; }
        public decimal Price { get; set; }
        public decimal StockQuantity { get; set; }
        public decimal MinStockQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
