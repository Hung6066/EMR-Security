using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web;
using EMRSystem.Application.Interfaces;

namespace EMRSystem.API.Middleware
{
    public class RaspMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RaspMiddleware> _logger;
        private readonly IServiceProvider _serviceProvider;

        public RaspMiddleware(RequestDelegate next, ILogger<RaspMiddleware> logger, IServiceProvider serviceProvider)
        {
            _next = next;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Bỏ qua các file tĩnh và các endpoint public không cần phân tích
            if (IsStaticFile(context.Request.Path) || IsPublicEndpoint(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // 1. Phân tích Request
            var (isThreat, reason) = await AnalyzeRequestAsync(context.Request);

            if (isThreat)
            {
                // 2. Phản ứng với mối đe dọa
                await TakeDefensiveActionAsync(context, reason);
                // Chặn request, không gọi _next
                return;
            }

            await _next(context);
        }

        private async Task<(bool IsThreat, string Reason)> AnalyzeRequestAsync(HttpRequest request)
        {
            var queryString = request.QueryString.HasValue ? HttpUtility.UrlDecode(request.QueryString.Value) : string.Empty;
            
            // Đọc body một cách an toàn
            string bodyAsString = string.Empty;
            if (request.ContentLength > 0 && request.HasJsonContentType())
            {
                request.EnableBuffering();
                using (var reader = new StreamReader(request.Body, leaveOpen: true))
                {
                    bodyAsString = await reader.ReadToEndAsync();
                    request.Body.Position = 0; // Reset stream để controller có thể đọc lại
                }
            }

            // Kiểm tra SQL Injection
            if (HasSqlInjection(queryString) || HasSqlInjection(bodyAsString))
                return (true, "Potential SQL Injection");

            // Kiểm tra Cross-Site Scripting (XSS)
            if (HasXss(queryString) || HasXss(bodyAsString))
                return (true, "Potential XSS");

            // Kiểm tra Path Traversal
            if (HasPathTraversal(request.Path.Value))
                return (true, "Path Traversal Attack");
            
            // (Thêm các kiểm tra khác ở đây)

            return (false, string.Empty);
        }

        private async Task TakeDefensiveActionAsync(HttpContext context, string reason)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            _logger.LogCritical(
                "RASP ALERT: {Reason} detected. User: {UserId}, IP: {IpAddress}, Path: {Path}, UserAgent: {UserAgent}",
                reason, userId, ipAddress, context.Request.Path, context.Request.Headers["User-Agent"]);

            // Sử dụng service scope để tránh lỗi "captive dependency"
            using (var scope = _serviceProvider.CreateScope())
            {
                var incidentService = scope.ServiceProvider.GetRequiredService<ISecurityIncidentService>();
                var threatIntelService = scope.ServiceProvider.GetRequiredService<IThreatIntelligenceService>();

                // 1. Tạo sự cố bảo mật
                await incidentService.CreateIncidentAsync(new CreateIncidentDto
                {
                    Title = $"RASP: {reason}",
                    Description = $"Request from IP {ipAddress} to {context.Request.Path} was blocked due to potential attack.",
                    Severity = "Critical",
                    Category = GetCategoryFromReason(reason),
                    IpAddress = ipAddress,
                    AffectedUserId = int.TryParse(userId, out var id) ? id : null,
                    IsAutoDetected = true,
                    DetectionRule = $"RASP-{reason.Replace(' ', '-')}"
                });

                // 2. Tự động chặn IP trong 1 giờ
                await threatIntelService.BlockIpAddressAsync(ipAddress, $"RASP Trigger: {reason}", TimeSpan.FromHours(1));
            }

            // 4. Trả về lỗi 400 Bad Request để che giấu lý do chặn
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid request format." });
        }

        // --- Các phương thức kiểm tra ---
        private bool HasSqlInjection(string? value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            // Regex patterns to detect SQL injection
            string[] patterns = {
                @"(%27)|(')|(--)|(%23)|(#)", // comments
                @"\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE){0,1}|INSERT( +INTO){0,1}|MERGE|SELECT|UPDATE|UNION( +ALL){0,1})\b",
                @"\b(OR|AND)\b\s+.*=",
                @"\b(xp_cmdshell|sp_executesql|sp_configure)\b"
            };
            return patterns.Any(p => Regex.IsMatch(value, p, RegexOptions.IgnoreCase | RegexOptions.Compiled));
        }

        private bool HasXss(string? value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            string[] patterns = {
                @"<script.*?>.*?</script.*?>",
                @"<img.*onerror\s*=\s*['""].*?['""].*?>",
                @"<iframe.*?>.*?</iframe.*?>",
                @"javascript:",
                @"on\w+\s*=" // onmouseover, onclick, etc.
            };
            // Decode trước khi kiểm tra
            var decodedValue = HttpUtility.UrlDecode(value);
            return patterns.Any(p => Regex.IsMatch(decodedValue, p, RegexOptions.IgnoreCase | RegexOptions.Compiled));
        }

        private bool HasPathTraversal(string? value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            return value.Contains("../") || value.Contains("..\\") || value.Contains("%2e%2e%2f") || value.Contains("%2e%2e\\");
        }

        private string GetCategoryFromReason(string reason)
        {
            if (reason.Contains("SQL")) return "SQL Injection";
            if (reason.Contains("XSS")) return "Cross-Site Scripting";
            if (reason.Contains("Path Traversal")) return "Path Traversal";
            return "Application Layer Attack";
        }
        
        private bool IsStaticFile(PathString path)
        {
            string[] staticExtensions = { ".css", ".js", ".jpg", ".png", ".svg", ".woff", ".woff2", ".ico" };
            return staticExtensions.Any(ext => path.Value?.EndsWith(ext, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private bool IsPublicEndpoint(PathString path)
        {
             var publicPaths = new[] { "/api/auth/", "/hangfire" };
             return publicPaths.Any(p => path.Value?.StartsWith(p, StringComparison.OrdinalIgnoreCase) ?? false);
        }
    }

    public static class RaspMiddlewareExtensions
    {
        public static IApplicationBuilder UseRasp(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RaspMiddleware>();
        }
    }
}