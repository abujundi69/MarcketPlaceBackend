namespace MarcketPlace.Application.Admin.Products.Dtos
{
    public class CreateAdminProductOptionDto
    {
        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public int SortOrder { get; set; }

        public List<CreateAdminProductOptionValueDto> Values { get; set; } = new();
    }
}