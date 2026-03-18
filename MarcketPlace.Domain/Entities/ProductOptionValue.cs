namespace MarcketPlace.Domain.Entities
{
    public class ProductOptionValue
    {
        public int Id { get; set; }
        public int ProductOptionId { get; set; }

        public string ValueAr { get; set; } = default!;
        public string ValueEn { get; set; } = default!;

        public string? ColorHex { get; set; }
        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }

        public ProductOption ProductOption { get; set; } = default!;
        public ICollection<ProductVariantOptionValue> VariantOptionValues { get; set; } = new List<ProductVariantOptionValue>();
    }
}