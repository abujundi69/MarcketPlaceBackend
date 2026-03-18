namespace MarcketPlace.Application.Admin.Products.Dtos
{
    public class AdminProductVariantSelectedValueDto
    {
        public int OptionId { get; set; }
        public string OptionNameAr { get; set; } = default!;
        public string OptionNameEn { get; set; } = default!;

        public int OptionValueId { get; set; }
        public string ValueAr { get; set; } = default!;
        public string ValueEn { get; set; } = default!;
        public string? ColorHex { get; set; }
    }
}