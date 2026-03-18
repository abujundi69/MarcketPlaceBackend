namespace MarcketPlace.Domain.Entities
{
    public class SystemSetting
    {
        public int Id { get; set; }

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

        public DateTime UpdatedAt { get; set; }

        public ICollection<MarketWorkingHour> WorkingHours { get; set; } = new List<MarketWorkingHour>();
    }
}