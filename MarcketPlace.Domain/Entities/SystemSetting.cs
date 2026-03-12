namespace MarcketPlace.Domain.Entities
{
    public class SystemSetting
    {
        public int Id { get; set; }
        public string SystemNameAr { get; set; } = default!;
        public string SystemNameEn { get; set; } = default!;
        public string FooterAr { get; set; } = default!;
        public string FooterEn { get; set; } = default!;

        /// <summary>
        /// رسالة ترويجية تظهر للعملاء في التطبيق (مثل: عرض خاص - توصيل مجاني فوق 50 شيكل).
        /// يتم تعديلها من لوحة تحكم الأدمن.
        /// </summary>
        public string? CustomerPromoMessage { get; set; }

        public byte[]? Logo { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}