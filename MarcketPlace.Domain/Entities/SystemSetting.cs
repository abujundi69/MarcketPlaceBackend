namespace MarcketPlace.Domain.Entities
{
    public class SystemSetting
    {
        public int Id { get; set; }
        public string SystemNameAr { get; set; } = default!;
        public string SystemNameEn { get; set; } = default!;
        public string FooterAr { get; set; } = default!;
        public string FooterEn { get; set; } = default!;
        public string? LogoUrl { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}