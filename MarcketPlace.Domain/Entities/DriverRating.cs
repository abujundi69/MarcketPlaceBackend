namespace MarcketPlace.Domain.Entities
{
    public class DriverRating
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int DriverId { get; set; }
        public int CustomerId { get; set; }

        public int Score { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public Order Order { get; set; } = default!;
        public Driver Driver { get; set; } = default!;
        public Customer Customer { get; set; } = default!;
    }
}