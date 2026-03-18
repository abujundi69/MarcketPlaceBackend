namespace MarcketPlace.Application.Auth.Dtos
{
    public class VerifyFirstLoginOtpRequestDto
    {
        public int OtpSessionId { get; set; }
        public string Code { get; set; } = default!;
    }
}