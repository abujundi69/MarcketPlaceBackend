namespace MarcketPlace.Application.Customer.Catalog.Dtos
{
    public class CustomerCategoryDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public byte[]? Image { get; set; }
        public int DisplayOrder { get; set; }
        public int? ParentId { get; set; }
        public int ProductsCount { get; set; }
    }
}