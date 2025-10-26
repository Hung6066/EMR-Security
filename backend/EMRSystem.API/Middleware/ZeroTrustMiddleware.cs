// ZeroTrustMiddleware.cs
namespace EMRSystem.API.Middleware
{
    public class ZeroTrustMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ZeroTrustMiddleware> _logger;

        public ZeroTrustMiddleware(RequestDelegate next, ILogger<ZeroTrustMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IZeroTrustService zeroTrustService,
            IDeviceFingerprintService deviceService)
        {
            // Skip for public endpoints
            if (IsPublicEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Skip if not authenticated
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await _next(context);
                return;
            }

            var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var resource = context.Request.Path.Value;
            var action = context.Request.Method;

            // Build access context
            var accessContext = new AccessContext
            {
                IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                DeviceFingerprint = context.Request.Headers["X-Device-Fingerprint"].ToString(),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                Timestamp = DateTime.Now,
                HasMFA = context.User.HasClaim("amr", "mfa"),
                CustomAttributes = new Dictionary<string, object>()
            };

            // Evaluate access
            var accessRequest = new AccessRequest
            {
                UserId = userId,
                Resource = resource,
                Action = action,
                Context = accessContext
            };

            var decision = await zeroTrustService.EvaluateAccessAsync(accessRequest);

            if (!decision.IsAllowed)
            {
                _logger.LogWarning(
                    $"Access denied for user {userId} to {resource}. Reasons: {string.Join(", ", decision.DenialReasons)}");

                context.Response.StatusCode = 403;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Access Denied",
                    reasons = decision.DenialReasons,
                    trustScore = decision.TrustScore,
                    requiredActions = decision.RequiredActions
                });
                return;
            }

            // Add trust score to response headers
            context.Response.Headers.Add("X-Trust-Score", decision.TrustScore.ToString());

            await _next(context);
        }

        private bool IsPublicEndpoint(PathString path)
        {
            var publicPaths = new[]
            {
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/forgot-password"
            };

            return publicPaths.Any(p => path.StartsWithSegments(p));
        }
    }

    public static class ZeroTrustMiddlewareExtensions
    {
        public static IApplicationBuilder UseZeroTrust(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ZeroTrustMiddleware>();
        }
    }
}