namespace MarcketPlace.Application.Admin.Products.Dtos
{
    public class CreateAdminProductOptionValueDto
    {

        public string Key { get; set; } = default!;

        public string ValueAr { get; set; } = default!;
        public string ValueEn { get; set; } = default!;
        public string? ColorHex { get; set; }
        public int SortOrder { get; set; }
    }
}