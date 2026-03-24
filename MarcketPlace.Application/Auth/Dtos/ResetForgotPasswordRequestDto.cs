namespace MarcketPlace.Application.Auth.Dtos
{
    public class ResetForgotPasswordRequestDto
    {
        public int OtpSessionId { get; set; }
        public string NewPassword { get; set; } = default!;
        public string ConfirmNewPassword { get; set; } = default!;
    }
}