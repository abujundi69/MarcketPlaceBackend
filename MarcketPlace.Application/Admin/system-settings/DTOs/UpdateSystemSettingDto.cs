namespace MarcketPlace.Application.Admin.SystemSettings.Dtos
{
    public class UpdateSystemSettingDto
    {
        public string SystemNameAr { get; set; } = default!;
        public string SystemNameEn { get; set; } = default!;
        public string FooterAr { get; set; } = default!;
        public string FooterEn { get; set; } = default!;

        public string? CustomerPromoMessage { get; set; }

        public byte[]? Logo { get; set; }

        public string PickupNameAr { get; set; } = default!;
        public string PickupNameEn { get; set; } = default!;
        public string PickupAddressText { get; set; } = default!;
        public decimal PickupLatitude { get; set; }
        public decimal PickupLongitude { get; set; }

        public List<UpdateSystemSettingWorkingHourDto> WorkingHours { get; set; } = new();
    }
}