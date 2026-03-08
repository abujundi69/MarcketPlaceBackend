using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Domain.Entities
{
    public class StoreWorkingHour
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public StoreDayEnum Day { get; set; }

        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public bool IsClosed { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Store Store { get; set; } = default!;
    }
}