namespace MarcketPlace.Application.Admin.Products.Dtos
{
    public class AdminProductOptionDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public int SortOrder { get; set; }

        public List<AdminProductOptionValueDto> Values { get; set; } = new();
    }
}