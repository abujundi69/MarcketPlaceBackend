namespace MarcketPlace.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? DefaultDeliveryZoneId { get; set; }
        public string? DefaultAddressText { get; set; }
        public decimal? DefaultLatitude { get; set; }
        public decimal? DefaultLongitude { get; set; }
        public DateTime? LocationUpdatedAt { get; set; }

        public User User { get; set; } = default!;
        public DeliveryZone? DefaultDeliveryZone { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<CustomerFavorite> Favorites { get; set; } = new List<CustomerFavorite>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<DriverRating> DriverRatings { get; set; } = new List<DriverRating>();
    }
}