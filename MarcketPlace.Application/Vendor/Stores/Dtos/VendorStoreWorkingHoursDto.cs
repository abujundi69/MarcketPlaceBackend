namespace MarcketPlace.Application.Vendor.Stores.Dtos
{
    public class VendorStoreWorkingHoursDto
    {
        public int StoreId { get; set; }
        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;

        public List<VendorStoreWorkingHourItemDto> WorkingHours { get; set; } = new();
    }
}