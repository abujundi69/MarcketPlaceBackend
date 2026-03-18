namespace MarcketPlace.Application.Auth.Dtos
{
    public class LoginResultDto
    {
        public bool RequiresOtp { get; set; }
        public int? OtpSessionId { get; set; }
        public string? Message { get; set; }
        public AuthResponseDto? Auth { get; set; }
    }
}