// ApiKeyAuthenticationHandler.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace EMRSystem.API.Authentication
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";
        public string Scheme => DefaultScheme;
        public string ApiKeyHeaderName { get; set; } = "X-API-Key";
    }

    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly IApiKeyService _apiKeyService;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IApiKeyService apiKeyService)
            : base(options, logger, encoder, clock)
        {
            _apiKeyService = apiKeyService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(Options.ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                return AuthenticateResult.NoResult();
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return AuthenticateResult.NoResult();
            }

            var ipAddress = Context.Connection.RemoteIpAddress?.ToString();
            var isValid = await _apiKeyService.ValidateApiKeyAsync(providedApiKey, ipAddress);

            if (!isValid)
            {
                return AuthenticateResult.Fail("Invalid API Key");
            }

            var keyPrefix = providedApiKey.Substring(0, 10);
            var apiKey = await _apiKeyService.GetApiKeyInfoAsync(keyPrefix);

            if (apiKey == null)
            {
                return AuthenticateResult.Fail("Invalid API Key");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, apiKey.UserId.ToString()),
                new Claim(ClaimTypes.Name, apiKey.User.FullName),
                new Claim("ApiKeyId", apiKey.Id.ToString()),
                new Claim("ApiKeyPrefix", apiKey.KeyPrefix)
            };

            // Add scopes as claims
            if (!string.IsNullOrEmpty(apiKey.AllowedScopes))
            {
                var scopes = apiKey.AllowedScopes.Split(',');
                foreach (var scope in scopes)
                {
                    claims.Add(new Claim("scope", scope.Trim()));
                }
            }

            var identity = new ClaimsIdentity(claims, Options.Scheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Options.Scheme);

            return AuthenticateResult.Success(ticket);
        }
    }

    public static class ApiKeyAuthenticationExtensions
    {
        public static AuthenticationBuilder AddApiKeySupport(
            this AuthenticationBuilder authenticationBuilder,
            Action<ApiKeyAuthenticationOptions> options = null)
        {
            return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationOptions.DefaultScheme,
                options);
        }
    }
}