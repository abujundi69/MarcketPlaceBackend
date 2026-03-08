namespace MarcketPlace.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;
        public string Type { get; set; } = default!;
        public int? ReferenceId { get; set; }
        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public User User { get; set; } = default!;
    }
}