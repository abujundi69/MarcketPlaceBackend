namespace MarcketPlace.Application.Admin.Categories.Dtos
{
    public class AdminCategoryDto
    {
        public int Id { get; set; }

        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;

        public byte[]? Image { get; set; }

        public int DisplayOrder { get; set; }

        public int? ParentId { get; set; }
        public string? ParentNameAr { get; set; }
        public string? ParentNameEn { get; set; }

        public bool IsActive { get; set; }

        public int ChildrenCount { get; set; }
        public int ProductsCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}