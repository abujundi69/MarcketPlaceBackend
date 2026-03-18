namespace MarcketPlace.Application.Auth
{
    public interface ITwilioVerifyService
    {
        Task SendCodeAsync(string phoneNumber);
        Task<bool> VerifyCodeAsync(string phoneNumber, string code);
    }
}