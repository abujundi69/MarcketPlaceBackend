namespace MarcketPlace.Application.Admin.ProductsDoscount.Dtos
{
    public class SetProductDiscountDto
    {
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public bool ApplyToDefaultProductOnly { get; set; } = true;
    }
}