namespace MarcketPlace.Application.Admin.Products.Dtos
{
    public class CreateAdminProductVariantDto
    {
        public int? UnitId { get; set; }

        public string? NameAr { get; set; }
        public string? NameEn { get; set; }

        public string? SKU { get; set; }
        public string? Barcode { get; set; }

        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal? CostPrice { get; set; }

        public decimal StockQuantity { get; set; }
        public decimal MinStockQuantity { get; set; } = 0m;

        public decimal? MinPurchaseQuantity { get; set; }
        public decimal? MaxPurchaseQuantity { get; set; }
        public decimal? QuantityStep { get; set; }

        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }

        public List<string> SelectedValueKeys { get; set; } = new();
    }
}