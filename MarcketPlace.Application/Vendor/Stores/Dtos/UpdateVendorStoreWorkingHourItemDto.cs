using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Vendor.Stores.Dtos
{
    public class UpdateVendorStoreWorkingHourItemDto
    {
        public StoreDayEnum Day { get; set; }
        public TimeSpan? OpenTime { get; set; }
        public TimeSpan? CloseTime { get; set; }
        public bool IsClosed { get; set; }
    }
}