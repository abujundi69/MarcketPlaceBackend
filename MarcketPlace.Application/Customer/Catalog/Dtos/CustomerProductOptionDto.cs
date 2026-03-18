namespace MarcketPlace.Application.Customer.Catalog.Dtos
{
    public class CustomerProductOptionDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public int SortOrder { get; set; }

        public List<CustomerProductOptionValueDto> Values { get; set; } = new();
    }
}