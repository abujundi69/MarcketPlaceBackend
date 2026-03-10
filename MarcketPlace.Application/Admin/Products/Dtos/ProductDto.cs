using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Admin.Products.Dtos
{
    public class ProductDto
    {
        public int Id { get; set; }
        public int? StoreId { get; set; }
        public int CategoryId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public string? ImageUrl { get; set; }

        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int MinStockQuantity { get; set; }

        public ProductApprovalStatus ApprovalStatus { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}