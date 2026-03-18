namespace MarcketPlace.Application.Admin.Notifications.Dtos
{
    public class AdminNotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;
        public string Type { get; set; } = default!;
        public int? ReferenceId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
