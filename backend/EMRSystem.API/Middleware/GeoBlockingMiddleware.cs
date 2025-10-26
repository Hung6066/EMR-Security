// GeoBlockingMiddleware.cs
namespace EMRSystem.API.Middleware
{
    public class GeoBlockingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeoBlockingMiddleware> _logger;

        // Blocked country codes (ISO 3166-1 alpha-2)
        private readonly HashSet<string> _blockedCountries;
        private readonly HashSet<string> _blockedIpRanges;

        public GeoBlockingMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<GeoBlockingMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;

            _blockedCountries = new HashSet<string>(
                configuration.GetSection("GeoBlocking:BlockedCountries").Get<string[]>() ?? Array.Empty<string>());
            
            _blockedIpRanges = new HashSet<string>(
                configuration.GetSection("GeoBlocking:BlockedIpRanges").Get<string[]>() ?? Array.Empty<string>());
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            if (!string.IsNullOrEmpty(ipAddress))
            {
                // Check if IP is in blocked ranges
                if (IsIpBlocked(ipAddress))
                {
                    _logger.LogWarning($"Blocked request from IP: {ipAddress}");
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { error = "Access denied from your location" });
                    return;
                }

                // Check country (would need IP geolocation service)
                var country = await GetCountryFromIpAsync(ipAddress);
                if (!string.IsNullOrEmpty(country) && _blockedCountries.Contains(country))
                {
                    _logger.LogWarning($"Blocked request from country: {country}, IP: {ipAddress}");
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsJsonAsync(new { error = "Access denied from your location" });
                    return;
                }
            }

            await _next(context);
        }

        private bool IsIpBlocked(string ipAddress)
        {
            foreach (var blockedRange in _blockedIpRanges)
            {
                if (IsIpInRange(ipAddress, blockedRange))
                    return true;
            }
            return false;
        }

        private bool IsIpInRange(string ipAddress, string range)
        {
            // Simple implementation - enhance with proper CIDR range checking
            if (range.Contains("/"))
            {
                // CIDR notation
                var parts = range.Split('/');
                // Implement CIDR matching logic
                return false;
            }
            else if (range.Contains("*"))
            {
                // Wildcard matching
                var pattern = "^" + Regex.Escape(range).Replace("\\*", ".*") + "$";
                return Regex.IsMatch(ipAddress, pattern);
            }
            else
            {
                return ipAddress == range;
            }
        }

        private async Task<string> GetCountryFromIpAsync(string ipAddress)
        {
            // Integrate with IP geolocation service (MaxMind, IPStack, etc.)
            // For demo, return null
            return null;
        }
    }
}