namespace MarcketPlace.Domain.Entities
{
    public class OtpCode
    {
        public int Id { get; set; } 
        public int UserId { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}