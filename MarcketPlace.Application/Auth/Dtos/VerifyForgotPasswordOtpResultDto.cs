namespace MarcketPlace.Application.Auth.Dtos
{
    public class VerifyForgotPasswordOtpResultDto
    {
        public bool CanResetPassword { get; set; }
        public int OtpSessionId { get; set; }
        public string Message { get; set; } = default!;
    }
}