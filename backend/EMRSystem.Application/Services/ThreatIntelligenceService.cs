// ThreatIntelligenceService.cs
namespace EMRSystem.Application.Services
{
    public class ThreatIntelligenceService : IThreatIntelligenceService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ThreatIntelligenceService> _logger;

        public ThreatIntelligenceService(
            ApplicationDbContext context,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ThreatIntelligenceService> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ThreatAssessment> AssessIpAddressAsync(string ipAddress)
        {
            var assessment = new ThreatAssessment
            {
                ThreatCategories = new List<string>(),
                RecentAbuses = new List<string>()
            };

            // Check local blacklist
            var isBlacklisted = await IsIpBlacklistedAsync(ipAddress);
            if (isBlacklisted)
            {
                assessment.IsThreat = true;
                assessment.ThreatScore = 100;
                assessment.ThreatCategories.Add("Blacklisted");
                return assessment;
            }

            // Check threat intelligence APIs
            var abuseDbScore = await CheckAbuseIPDBAsync(ipAddress);
            var vpnCheck = await CheckVPNAsync(ipAddress);

            assessment.ThreatScore = abuseDbScore;
            assessment.IsVPN = vpnCheck.IsVPN;
            assessment.IsProxy = vpnCheck.IsProxy;
            assessment.IsTor = vpnCheck.IsTor;
            assessment.IsDataCenter = vpnCheck.IsDataCenter;
            assessment.CountryCode = vpnCheck.CountryCode;

            if (assessment.ThreatScore > 50)
            {
                assessment.IsThreat = true;
                assessment.ThreatCategories.Add("High Risk IP");
            }

            if (assessment.IsVPN || assessment.IsProxy || assessment.IsTor)
            {
                assessment.ThreatScore += 30;
                assessment.ThreatCategories.Add("Anonymization");
            }

            // Log threat
            await LogThreatAsync(ipAddress, assessment);

            return assessment;
        }

        public async Task<bool> IsIpBlacklistedAsync(string ipAddress)
        {
            var blocked = await _context.IpBlacklists
                .AnyAsync(b => b.IpAddress == ipAddress && 
                              b.IsActive &&
                              (!b.ExpiresAt.HasValue || b.ExpiresAt.Value > DateTime.Now));

            return blocked;
        }

        public async Task ReportSuspiciousActivityAsync(SuspiciousActivityReport report)
        {
            var threatLog = new ThreatLog
            {
                IpAddress = report.IpAddress,
                ThreatType = report.ActivityType,
                Description = report.Description,
                Metadata = JsonSerializer.Serialize(report.Metadata),
                DetectedAt = DateTime.Now,
                Severity = CalculateSeverity(report.ActivityType),
                IsBlocked = false
            };

            _context.ThreatLogs.Add(threatLog);
            await _context.SaveChangesAsync();

            // Auto-block if severity is high
            if (threatLog.Severity >= 8)
            {
                await BlockIpAddressAsync(
                    report.IpAddress, 
                    $"Auto-blocked: {report.ActivityType}", 
                    TimeSpan.FromHours(24)
                );
            }

            _logger.LogWarning(
                $"Suspicious activity reported: {report.ActivityType} from {report.IpAddress}");
        }

        public async Task<List<ThreatIndicator>> GetActiveThreatIndicatorsAsync()
        {
            var recentThreats = await _context.ThreatLogs
                .Where(t => t.DetectedAt > DateTime.Now.AddHours(-24) && t.Severity >= 7)
                .GroupBy(t => new { t.IpAddress, t.ThreatType })
                .Select(g => new ThreatIndicator
                {
                    Type = g.Key.ThreatType,
                    Value = g.Key.IpAddress,
                    Severity = g.Max(t => t.Severity),
                    DetectedAt = g.Max(t => t.DetectedAt)
                })
                .OrderByDescending(t => t.Severity)
                .Take(100)
                .ToListAsync();

            return recentThreats;
        }

        public async Task BlockIpAddressAsync(string ipAddress, string reason, TimeSpan? duration = null)
        {
            var existing = await _context.IpBlacklists
                .FirstOrDefaultAsync(b => b.IpAddress == ipAddress && b.IsActive);

            if (existing != null)
            {
                existing.ExpiresAt = duration.HasValue 
                    ? DateTime.Now.Add(duration.Value) 
                    : null;
                existing.Reason = reason;
            }
            else
            {
                var blacklist = new IpBlacklist
                {
                    IpAddress = ipAddress,
                    Reason = reason,
                    BlockedAt = DateTime.Now,
                    ExpiresAt = duration.HasValue ? DateTime.Now.Add(duration.Value) : null,
                    BlockedBy = 1, // System user
                    IsActive = true
                };

                _context.IpBlacklists.Add(blacklist);
            }

            await _context.SaveChangesAsync();

            _logger.LogWarning($"IP address blocked: {ipAddress} - Reason: {reason}");
        }

        private async Task<int> CheckAbuseIPDBAsync(string ipAddress)
        {
            try
            {
                var apiKey = _configuration["AbuseIPDB:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                    return 0;

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Key", apiKey);
                _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

                var response = await _httpClient.GetAsync(
                    $"https://api.abuseipdb.com/api/v2/check?ipAddress={ipAddress}&maxAgeInDays=90");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AbuseIPDBResponse>(content);
                    return result?.Data?.AbuseConfidenceScore ?? 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking AbuseIPDB for {ipAddress}");
            }

            return 0;
        }

        private async Task<(bool IsVPN, bool IsProxy, bool IsTor, bool IsDataCenter, string CountryCode)> 
            CheckVPNAsync(string ipAddress)
        {
            try
            {
                var apiKey = _configuration["IPQualityScore:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                    return (false, false, false, false, "");

                var response = await _httpClient.GetAsync(
                    $"https://ipqualityscore.com/api/json/ip/{apiKey}/{ipAddress}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<IPQualityScoreResponse>(content);
                    
                    return (
                        result?.VPN ?? false,
                        result?.Proxy ?? false,
                        result?.Tor ?? false,
                        result?.IsDataCenter ?? false,
                        result?.CountryCode ?? ""
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking VPN for {ipAddress}");
            }

            return (false, false, false, false, "");
        }

        private async Task LogThreatAsync(string ipAddress, ThreatAssessment assessment)
        {
            if (!assessment.IsThreat) return;

            var threatLog = new ThreatLog
            {
                IpAddress = ipAddress,
                ThreatType = string.Join(", ", assessment.ThreatCategories),
                Severity = assessment.ThreatScore / 10,
                Description = $"Threat score: {assessment.ThreatScore}",
                Metadata = JsonSerializer.Serialize(assessment),
                DetectedAt = DateTime.Now,
                IsBlocked = false
            };

            _context.ThreatLogs.Add(threatLog);
            await _context.SaveChangesAsync();
        }

        private int CalculateSeverity(string activityType)
        {
            return activityType switch
            {
                "BruteForce" => 9,
                "SQLInjection" => 10,
                "XSS" => 8,
                "CSRF" => 7,
                "UnauthorizedAccess" => 8,
                "DataExfiltration" => 10,
                _ => 5
            };
        }
    }

    // Response models for external APIs
    public class AbuseIPDBResponse
    {
        public AbuseIPDBData Data { get; set; }
    }

    public class AbuseIPDBData
    {
        public int AbuseConfidenceScore { get; set; }
    }

    public class IPQualityScoreResponse
    {
        public bool VPN { get; set; }
        public bool Proxy { get; set; }
        public bool Tor { get; set; }
        public bool IsDataCenter { get; set; }
        public string CountryCode { get; set; }
    }
}