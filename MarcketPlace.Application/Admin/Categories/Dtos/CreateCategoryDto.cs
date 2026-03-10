namespace MarcketPlace.Application.Admin.Categories.Dtos
{
    public class CreateCategoryDto
    {
        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public int? ParentId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}