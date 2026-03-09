namespace MarcketPlace.Domain.Entities
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Customer Customer { get; set; } = default!;
        public Product Product { get; set; } = default!;
    }
}