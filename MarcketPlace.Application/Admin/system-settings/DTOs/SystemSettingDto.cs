namespace MarcketPlace.Application.Admin.SystemSettings.Dtos
{
    public class SystemSettingDto
    {
        public int Id { get; set; }
        public string SystemNameAr { get; set; } = default!;
        public string SystemNameEn { get; set; } = default!;
        public string FooterAr { get; set; } = default!;
        public string FooterEn { get; set; } = default!;

        public byte[]? Logo { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}