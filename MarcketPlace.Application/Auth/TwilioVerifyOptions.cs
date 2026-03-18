namespace MarcketPlace.Application.Auth
{
    public class TwilioVerifyOptions
    {
        public string AccountSid { get; set; } = default!;
        public string AuthToken { get; set; } = default!;
        public string VerifyServiceSid { get; set; } = default!;
    }
}