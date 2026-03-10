namespace MarcketPlace.Application.Admin.SystemSettings.Dtos
{
    public class UpdateSystemSettingDto
    {
        public string SystemNameAr { get; set; } = default!;
        public string SystemNameEn { get; set; } = default!;
        public string FooterAr { get; set; } = default!;
        public string FooterEn { get; set; } = default!;

        public byte[]? Logo { get; set; }
    }
}