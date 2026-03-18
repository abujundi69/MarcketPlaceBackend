using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Customer.Catalog.Dtos
{
    public class CustomerMostOrderedProductDto
    {
        public int ProductId { get; set; }

        public int? DefaultVariantId { get; set; }
        public string? DefaultVariantNameAr { get; set; }
        public string? DefaultVariantNameEn { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public byte[]? Image { get; set; }

        public ProductType ProductType { get; set; }
        public ProductPurchaseInputMode PurchaseInputMode { get; set; }
        public bool AllowDecimalQuantity { get; set; }

        public string? UnitSymbol { get; set; }

        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal EffectivePrice { get; set; }

        public bool IsAvailable { get; set; }

        public int OrdersCount { get; set; }
        public decimal TotalOrderedQuantity { get; set; }
    }
}