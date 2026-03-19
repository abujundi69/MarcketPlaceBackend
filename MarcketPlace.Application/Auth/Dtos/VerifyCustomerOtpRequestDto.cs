namespace MarcketPlace.Application.Auth.Dtos
{
    public class VerifyCustomerOtpRequestDto
    {
        public int OtpSessionId { get; set; }
        public string Code { get; set; } = default!;
    }
}