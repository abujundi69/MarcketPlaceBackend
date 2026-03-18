namespace MarcketPlace.Domain.Entities
{
    public class ProductVariantOptionValue
    {
        public int Id { get; set; }

        public int ProductVariantId { get; set; }
        public int ProductOptionValueId { get; set; }

        public ProductVariant ProductVariant { get; set; } = default!;
        public ProductOptionValue ProductOptionValue { get; set; } = default!;
    }
}