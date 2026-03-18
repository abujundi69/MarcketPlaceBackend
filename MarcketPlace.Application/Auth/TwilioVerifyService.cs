using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Verify.V2.Service;

namespace MarcketPlace.Application.Auth
{
    public class TwilioVerifyService : ITwilioVerifyService
    {
        private readonly TwilioVerifyOptions _options;

        public TwilioVerifyService(IOptions<TwilioVerifyOptions> options)
        {
            _options = options.Value;
            TwilioClient.Init(_options.AccountSid, _options.AuthToken);
        }

        public async Task SendCodeAsync(string phoneNumber)
        {
            try
            {
                var verification = await VerificationResource.CreateAsync(
                    to: phoneNumber,
                    channel: "sms",
                    pathServiceSid: _options.VerifyServiceSid
                );

                if (!string.Equals(verification.Status, "pending", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("فشل إرسال رمز التحقق.");
            }
            catch (ApiException ex)
            {
                throw new Exception($"Twilio error: {ex.Message}");
            }
        }

        public async Task<bool> VerifyCodeAsync(string phoneNumber, string code)
        {
            try
            {
                var check = await VerificationCheckResource.CreateAsync(
                    to: phoneNumber,
                    code: code,
                    pathServiceSid: _options.VerifyServiceSid
                );

                return string.Equals(check.Status, "approved", StringComparison.OrdinalIgnoreCase);
            }
            catch (ApiException ex)
            {
                throw new Exception($"Twilio error: {ex.Message}");
            }
        }
    }
}