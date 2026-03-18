using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Admin.SystemSettings.Dtos
{
    public class UpdateSystemSettingWorkingHourDto
    {
        public WeekDayEnum Day { get; set; }

        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public bool IsClosed { get; set; }
    }
}