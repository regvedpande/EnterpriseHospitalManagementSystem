using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Hospital.Utilities
{
    public class TwilioSmsService : ISmsService
    {
        private readonly IConfiguration _config;

        public TwilioSmsService(IConfiguration config) => _config = config;

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            var sid   = _config["Twilio:AccountSid"];
            var token = _config["Twilio:AuthToken"];
            var from  = _config["Twilio:From"];

            if (string.IsNullOrWhiteSpace(sid) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(from))
                return; // SMS not configured; skip silently

            TwilioClient.Init(sid, token);
            await MessageResource.CreateAsync(
                to:   new Twilio.Types.PhoneNumber(phoneNumber),
                from: new Twilio.Types.PhoneNumber(from),
                body: message);
        }
    }
}
