namespace MarcketPlace.Domain.Entities
{
    public class StoreRating
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int StoreId { get; set; }
        public int CustomerId { get; set; }

        public int Score { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public Order Order { get; set; } = default!;
        public Store Store { get; set; } = default!;
        public Customer Customer { get; set; } = default!;
    }
}