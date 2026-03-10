namespace MarcketPlace.Application.Admin.DeliveryZones.Dtos
{
    public class DeliveryZoneDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public decimal DeliveryFee { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}