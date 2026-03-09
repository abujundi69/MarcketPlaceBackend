namespace MarcketPlace.Domain.Entities
{
    public class Vendor
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = default!;
        public ICollection<Store> Stores { get; set; } = new List<Store>();
    }
}