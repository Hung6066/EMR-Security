public class DeceptionService : IDeceptionService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DeceptionService> _logger;

    public DeceptionService(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor, ILogger<DeceptionService> logger)
    {
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task TriggerHoneypotAsync(string honeypotName, string description)
    {
        var context = _httpContextAccessor.HttpContext;
        var userId = context?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ipAddress = context?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        _logger.LogCritical("HONEYPOT TRIGGERED: {HoneypotName}. User: {UserId}, IP: {IpAddress}, Details: {Description}",
            honeypotName, userId ?? "Anonymous", ipAddress, description);
            
        using var scope = _serviceProvider.CreateScope();
        var incidentService = scope.ServiceProvider.GetRequiredService<ISecurityIncidentService>();
        var threatIntelService = scope.ServiceProvider.GetRequiredService<IThreatIntelligenceService>();

        // 1. Tạo sự cố bảo mật với mức độ nghiêm trọng cao nhất
        await incidentService.CreateIncidentAsync(new CreateIncidentDto
        {
            Title = $"Honeypot Triggered: {honeypotName}",
            Description = description,
            Severity = "Critical", // Bất kỳ tương tác nào với bẫy đều là Critical
            Category = "Reconnaissance",
            IpAddress = ipAddress,
            AffectedUserId = int.TryParse(userId, out var id) ? id : null,
            IsAutoDetected = true,
        });

        // 2. Chặn IP ngay lập tức và vĩnh viễn
        await threatIntelService.BlockIpAddressAsync(ipAddress, $"Honeypot Trigger: {honeypotName}");
    }
}