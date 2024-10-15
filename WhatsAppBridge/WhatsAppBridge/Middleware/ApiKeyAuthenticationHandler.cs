using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using WhatsAppBridge.Settings;

namespace WhatsAppBridge.Middleware
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string ApiKeyHeaderName = "X-API-KEY";
        private string ApiKey = String.Empty; // Hardcoded or fetched from config.

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            IOptions<AuthenticationConfigurationSettings> authenticationConfigurationSettings,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
            ApiKey = authenticationConfigurationSettings.Value.ApiKey;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("API Key was not provided."));
            }

            if (!ApiKey.Equals(extractedApiKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key provided."));
            }

            var claims = new[] { new Claim(ClaimTypes.Name, "ApiKeyUser") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
