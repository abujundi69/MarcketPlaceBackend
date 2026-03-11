namespace MarcketPlace.Application.Vendor.Stores.Dtos
{
    public class UpdateVendorStoreWorkingHoursDto
    {
        public List<UpdateVendorStoreWorkingHourItemDto> WorkingHours { get; set; } = new();
    }
}