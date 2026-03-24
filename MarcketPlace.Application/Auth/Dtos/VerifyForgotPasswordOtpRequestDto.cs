namespace MarcketPlace.Application.Auth.Dtos
{
    public class VerifyForgotPasswordOtpRequestDto
    {
        public int OtpSessionId { get; set; }
        public string Code { get; set; } = default!;
    }
}