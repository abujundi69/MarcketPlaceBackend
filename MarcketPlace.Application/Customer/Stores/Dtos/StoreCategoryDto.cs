namespace MarcketPlace.Application.Customer.Stores.Dtos
{
    /// <summary>فئة للمتاجر — للفلترة في شاشة العميل.</summary>
    public class StoreCategoryDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
    }
}
