namespace MarcketPlace.Domain.Entities
{
    public class Store
    {
        public int Id { get; set; }
        public int? VendorId { get; set; }
        public string NameAr { get; set; } = default!;
        public string NameEn { get; set; } = default!;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public string AddressText { get; set; } = default!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Vendor? Vendor { get; set; }
        public ICollection<StoreWorkingHour> WorkingHours { get; set; } = new List<StoreWorkingHour>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<OrderStore> OrderStores { get; set; } = new List<OrderStore>();
        public ICollection<StoreRating> StoreRatings { get; set; } = new List<StoreRating>();
    }
}