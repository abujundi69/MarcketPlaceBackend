using MarcketPlace.Domain.Enums;

namespace MarcketPlace.Application.Account.Dtos
{
    public class MyProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}