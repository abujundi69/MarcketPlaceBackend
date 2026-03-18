using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Domain.Entities
{
    public class ProductUnit
    {
        public int Id { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string Symbol { get; set; } = default!;

        public ProductMeasurementType MeasurementType { get; set; }

        public decimal FactorToBaseUnit { get; set; } = 1m;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
    }
}