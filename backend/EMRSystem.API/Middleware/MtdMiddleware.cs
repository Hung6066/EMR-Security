public class MtdMiddleware
{
    private readonly RequestDelegate _next;
    // Key bí mật để tạo hash, lưu trong appsettings
    private readonly string _routingSecret;
    private readonly ILogger<MtdMiddleware> _logger;

    public MtdMiddleware(RequestDelegate next, IConfiguration config, ILogger<MtdMiddleware> logger)
    {
        _next = next;
        _routingSecret = config["MtdSettings:RoutingSecret"]!;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";
        
        // Ví dụ: request đến /api/rt-a3b1/patients
        var mtdPrefix = "/api/rt-"; 
        if (path.StartsWith(mtdPrefix))
        {
            var segments = path.Substring(mtdPrefix.Length).Split('/', 2);
            if (segments.Length == 2)
            {
                var dynamicPart = segments[0];
                var realResource = segments[1];

                // Xác thực dynamicPart
                if (IsValidDynamicRoute(dynamicPart, realResource))
                {
                    // Ánh xạ lại đường dẫn
                    var newPath = $"/api/{realResource}";
                    _logger.LogInformation("MTD: Rewriting path from {OldPath} to {NewPath}", path, newPath);
                    context.Request.Path = newPath;
                }
                else
                {
                    _logger.LogWarning("MTD: Invalid dynamic route detected: {Path}", path);
                    context.Response.StatusCode = 404;
                    return;
                }
            }
        }
        await _next(context);
    }

    // Tạo ra một hash ngắn dựa trên tên resource và một yếu tố thời gian (ví dụ: ngày)
    private string GenerateDynamicPart(string resource)
    {
        // Yếu tố thời gian, thay đổi mỗi ngày
        var timeFactor = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var dataToHash = $"{resource}:{timeFactor}:{_routingSecret}";
        
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
        
        // Trả về 4 ký tự đầu của hash
        return BitConverter.ToString(hash).Replace("-", "").Substring(0, 4).ToLowerInvariant();
    }

    private bool IsValidDynamicRoute(string dynamicPart, string resource)
    {
        // Kiểm tra hash của ngày hôm nay và ngày hôm qua (để tránh lỗi ở thời điểm giao ngày)
        var todayPart = GenerateDynamicPart(resource);
        var yesterdayPart = GenerateDynamicPart(resource, DateTime.UtcNow.AddDays(-1));
        
        return dynamicPart == todayPart || dynamicPart == yesterdayPart;
    }

    private string GenerateDynamicPart(string resource, DateTime time)
    {
        var timeFactor = time.ToString("yyyy-MM-dd");
        var dataToHash = $"{resource}:{timeFactor}:{_routingSecret}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToHash));
        return BitConverter.ToString(hash).Replace("-", "").Substring(0, 4).ToLowerInvariant();
    }
}

public static class MtdMiddlewareExtensions
{
    public static IApplicationBuilder UseMovingTargetDefense(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MtdMiddleware>();
    }
}