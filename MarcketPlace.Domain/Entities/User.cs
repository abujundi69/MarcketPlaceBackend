using System.Text.Json.Serialization;
using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;

        [JsonIgnore]
        public string PasswordHash { get; set; } = default!;

        public UserRole Role { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public Vendor? Vendor { get; set; }
        public Driver? Driver { get; set; }
        public Customer? Customer { get; set; }

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}