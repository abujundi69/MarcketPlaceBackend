namespace MarcketPlace.Domain.Entities
{
    public class Customer
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = default!;
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<StoreRating> StoreRatings { get; set; } = new List<StoreRating>();
        public ICollection<DriverRating> DriverRatings { get; set; } = new List<DriverRating>();
    }
}