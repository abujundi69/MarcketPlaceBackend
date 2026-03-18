namespace MarcketPlace.Domain.Entities
{
    public class ProductOption
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Product Product { get; set; } = default!;
        public ICollection<ProductOptionValue> Values { get; set; } = new List<ProductOptionValue>();
    }
}