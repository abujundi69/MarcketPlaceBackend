using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Users.Dtos
{
    public class UserListItemDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;

        public UserRole Role { get; set; }
        public string RoleName { get; set; } = default!;

        public bool IsActive { get; set; }
        public string AccountStatus { get; set; } = default!;

        public DateTime CreatedAt { get; set; }
        public string CreatedAtText { get; set; } = default!;
    }
}