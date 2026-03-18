namespace MarcketPlace.Application.Admin.Products.Dtos
{
    public class AdminProductOptionValueDto
    {
        public int Id { get; set; }
        public string ValueAr { get; set; } = default!;
        public string ValueEn { get; set; } = default!;
        public string? ColorHex { get; set; }
        public int SortOrder { get; set; }
    }
}