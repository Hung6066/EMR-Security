using EMRSystem.Application.Interfaces;
using EMRSystem.Core.Entities;
using EMRSystem.Core.Entities.Security;
using EMRSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace EMRSystem.Application.Services;

public class UebaService : IUebaService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UebaService> _logger;
    private readonly ISecurityIncidentService _incidentService;

    public UebaService(ApplicationDbContext context, ILogger<UebaService> logger, ISecurityIncidentService incidentService)
    {
        _context = context;
        _logger = logger;
        _incidentService = incidentService;
    }

    public async Task UpdateBehavioralBaselinesAsync()
    {
        var userIds = await _context.Users.Where(u => u.IsActive).Select(u => u.Id).ToListAsync();
        _logger.LogInformation("Starting UEBA baseline update for {UserCount} active users.", userIds.Count);

        foreach (var userId in userIds)
        {
            try
            {
                var profile = await _context.UserBehaviorProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
                              ?? new UserBehaviorProfile { UserId = userId };

                var recentActivity = await _context.AuditLogs
                    .Where(a => a.UserId == userId && a.Timestamp > DateTime.UtcNow.AddDays(-90))
                    .OrderByDescending(a => a.Timestamp)
                    .Take(5000)
                    .AsNoTracking()
                    .ToListAsync();

                if (recentActivity.Count < 50) continue;

                // Học baseline mới
                profile.TypicalLoginHours = CalculateTypicalItems(recentActivity.Where(a => a.Action == "LOGIN_SUCCESS").Select(a => a.Timestamp.ToUniversalTime().Hour.ToString()));
                profile.TypicalIpSubnets = CalculateTypicalSubnets(recentActivity.Select(a => a.IpAddress));
                profile.FrequentActions = CalculateTypicalItems(recentActivity.Select(a => a.Action));
                profile.FrequentResources = CalculateTypicalItems(recentActivity.Select(a => a.EntityType));
                
                profile.LastUpdatedAt = DateTime.UtcNow;
                
                if (profile.Id == 0) _context.UserBehaviorProfiles.Add(profile);
                else _context.UserBehaviorProfiles.Update(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update UEBA baseline for user {UserId}", userId);
            }
        }
        await _context.SaveChangesAsync();
        _logger.LogInformation("UEBA baselines update completed.");
    }

    public async Task AnalyzeAndAlertOnActivityAsync(AuditLog log)
    {
        if (log.UserId == null) return;

        var profile = await _context.UserBehaviorProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == log.UserId);
        if (profile == null) return;

        var reasons = new List<string>();

        var hourDeviation = AnalyzeDeviation(log.Timestamp.ToUniversalTime().Hour.ToString(), profile.TypicalLoginHours, 0.8);
        if (hourDeviation.IsAnomalous) reasons.Add($"Unusual Time ({log.Timestamp.ToLocalTime():HH:mm})");

        var ipDeviation = AnalyzeIpDeviation(log.IpAddress, profile.TypicalIpSubnets);
        if (ipDeviation.IsAnomalous) reasons.Add($"Unusual Location/IP ({log.IpAddress})");

        var actionDeviation = AnalyzeDeviation(log.Action, profile.FrequentActions, 0.9);
        if (actionDeviation.IsAnomalous) reasons.Add($"Unusual Action ({log.Action})");
        
        var resourceDeviation = AnalyzeDeviation(log.EntityType, profile.FrequentResources, 0.9);
        if (resourceDeviation.IsAnomalous) reasons.Add($"Unusual Resource ({log.EntityType})");
        
        // Tính điểm tổng hợp có trọng số
        var totalDeviation = (hourDeviation.Score * 0.25) + (ipDeviation.Score * 0.40) + (actionDeviation.Score * 0.15) + (resourceDeviation.Score * 0.20);
        
        if (totalDeviation > 0.75) // Ngưỡng cảnh báo
        {
            var alert = new UebaAlert
            {
                UserId = log.UserId.Value,
                AlertType = "BehavioralDeviation",
                Description = string.Join(", ", reasons),
                DeviationScore = totalDeviation,
                DetectedAt = DateTime.UtcNow,
                Context = JsonSerializer.Serialize(new { log.Action, log.EntityType, log.EntityId, log.IpAddress, log.Id }),
                Status = "New"
            };
            _context.UebaAlerts.Add(alert);
            await _context.SaveChangesAsync();

            if (totalDeviation > 0.9)
            {
                await _incidentService.CreateIncidentAsync(new CreateIncidentDto
                {
                    Title = "UEBA: High-Risk Behavior Detected",
                    Description = $"User '{log.User?.UserName}' exhibited highly anomalous behavior. Score: {totalDeviation:P2}. Details: {alert.Description}",
                    Severity = "High",
                    Category = "InsiderThreat",
                    AffectedUserId = log.UserId,
                    IpAddress = log.IpAddress,
                    IsAutoDetected = true
                });
            }
        }
    }

    public Task<List<UebaAlert>> GetAlertsAsync(DateTime from, DateTime to) =>
        _context.UebaAlerts.Include(a => a.User).Where(a => a.DetectedAt >= from && a.DetectedAt <= to)
            .OrderByDescending(a => a.DetectedAt).ToListAsync();

    public Task<UebaAlert?> GetAlertByIdAsync(long id) =>
        _context.UebaAlerts.Include(a => a.User).FirstOrDefaultAsync(a => a.Id == id);

    public async Task UpdateAlertStatusAsync(long id, string status) {
        var alert = await _context.UebaAlerts.FindAsync(id);
        if (alert != null) {
            alert.Status = status;
            await _context.SaveChangesAsync();
        }
    }
    
    public async Task<UebaMetrics> GetMetricsAsync(DateTime from, DateTime to) {
        var query = _context.UebaAlerts.Where(a => a.DetectedAt >= from && a.DetectedAt <= to);
        return new UebaMetrics {
            TotalAlerts = await query.CountAsync(),
            UnresolvedAlerts = await query.CountAsync(a => a.Status == "New" || a.Status == "Investigating"),
            AvgDeviationScore = await query.AnyAsync() ? await query.AverageAsync(a => a.DeviationScore) : 0,
            AlertsByType = await query.GroupBy(a => a.AlertType).ToDictionaryAsync(g => g.Key, g => g.Count())
        };
    }

    // --- Private Helper Methods ---
    private string CalculateTypicalItems(IEnumerable<string> items) {
        var itemCounts = items.Where(i => !string.IsNullOrEmpty(i))
            .GroupBy(i => i).Select(g => new { Item = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).Take(10);
        return JsonSerializer.Serialize(itemCounts.ToDictionary(x => x.Item, x => x.Count));
    }
    private string CalculateTypicalSubnets(IEnumerable<string> ips) {
        var subnets = ips.Select(ip => {
                if (!IPAddress.TryParse(ip, out var addr)) return null;
                var parts = addr.ToString().Split('.');
                return (parts.Length == 4) ? $"{parts[0]}.{parts[1]}.{parts[2]}.0/24" : null;
            }).Where(s => s != null).GroupBy(s => s!)
            .Select(g => new { Subnet = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).Take(5);
        return JsonSerializer.Serialize(subnets.ToDictionary(x => x.Subnet, x => x.Count));
    }
    private (bool IsAnomalous, double Score) AnalyzeDeviation(string currentItem, string baselineJson, double threshold) {
        if (string.IsNullOrEmpty(baselineJson)) return (false, 0.5);
        try {
            var baseline = JsonSerializer.Deserialize<Dictionary<string, int>>(baselineJson);
            if (baseline == null || !baseline.Any()) return (false, 0.5);
            var total = (double)baseline.Values.Sum();
            var frequency = baseline.ContainsKey(currentItem) ? baseline[currentItem] / total : 0;
            var score = 1.0 - frequency;
            return (score > threshold, score);
        } catch { return (false, 0.5); }
    }
    private (bool IsAnomalous, double Score) AnalyzeIpDeviation(string currentIp, string subnetBaselineJson) {
        if (string.IsNullOrEmpty(subnetBaselineJson) || !IPAddress.TryParse(currentIp, out var addr)) return (false, 0.5);
        try {
            var baseline = JsonSerializer.Deserialize<Dictionary<string, int>>(subnetBaselineJson);
            if (baseline == null || !baseline.Any()) return (false, 0.5);
            var parts = addr.ToString().Split('.');
            if (parts.Length != 4) return (true, 0.9);
            var currentSubnet = $"{parts[0]}.{parts[1]}.{parts[2]}.0/24";
            var isKnown = baseline.ContainsKey(currentSubnet);
            return (!isKnown, isKnown ? 0.1 : 0.9);
        } catch { return (false, 0.5); }
    }
}