using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Domain.Entities
{
    public class MarketWorkingHour
    {
        public int Id { get; set; }

        public int SystemSettingId { get; set; }
        public WeekDayEnum Day { get; set; }

        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public bool IsClosed { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public SystemSetting SystemSetting { get; set; } = default!;
    }
}