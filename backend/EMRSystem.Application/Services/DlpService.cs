// DlpService.cs
using System.Text.RegularExpressions;

namespace EMRSystem.Application.Services
{
    public class DlpService : IDlpService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILogger<DlpService> _logger;

        private readonly Dictionary<DlpScanType, string> _patterns = new()
        {
            { DlpScanType.Email, @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b" },
            { DlpScanType.PhoneNumber, @"\b(\+84|0)[0-9]{9,10}\b" },
            { DlpScanType.IdentityCard, @"\b\d{9,12}\b" },
            { DlpScanType.CreditCard, @"\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b" }
        };

        public DlpService(
            ApplicationDbContext context,
            IAuditService auditService,
            ILogger<DlpService> logger)
        {
            _context = context;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<DlpScanResult> ScanContentAsync(string content, DlpScanType scanType)
        {
            var result = new DlpScanResult
            {
                Matches = new List<SensitiveDataMatch>(),
                RedactedContent = content
            };

            var patternsToCheck = scanType == DlpScanType.All 
                ? _patterns 
                : _patterns.Where(p => p.Key == scanType).ToDictionary(p => p.Key, p => p.Value);

            foreach (var pattern in patternsToCheck)
            {
                var matches = Regex.Matches(content, pattern.Value);
                
                foreach (Match match in matches)
                {
                    result.Matches.Add(new SensitiveDataMatch
                    {
                        Type = pattern.Key,
                        Value = match.Value,
                        Position = match.Index
                    });

                    // Redact sensitive data
                    var redactedValue = RedactValue(match.Value, pattern.Key);
                    result.RedactedContent = result.RedactedContent.Replace(match.Value, redactedValue);
                }
            }

            result.HasSensitiveData = result.Matches.Any();

            if (result.HasSensitiveData)
            {
                _logger.LogWarning($"Sensitive data detected: {result.Matches.Count} matches");
            }

            return result;
        }

        public async Task<bool> ValidateDataExportAsync(int userId, string dataType, int recordCount)
        {
            // Check export limits
            var today = DateTime.Today;
            var exportsToday = await _context.AuditLogs
                .Where(a => a.UserId == userId && 
                           a.Action == "EXPORT" && 
                           a.EntityType == dataType &&
                           a.Timestamp >= today)
                .CountAsync();

            const int maxExportsPerDay = 10;
            const int maxRecordsPerExport = 100;

            if (exportsToday >= maxExportsPerDay)
            {
                _logger.LogWarning($"User {userId} exceeded daily export limit");
                return false;
            }

            if (recordCount > maxRecordsPerExport)
            {
                _logger.LogWarning($"User {userId} attempted to export too many records: {recordCount}");
                return false;
            }

            return true;
        }

        public async Task LogDataExportAsync(int userId, string dataType, int recordCount, string destination)
        {
            await _auditService.LogAsync(new AuditLogDto
            {
                UserId = userId,
                Action = "EXPORT",
                EntityType = dataType,
                AdditionalInfo = $"Exported {recordCount} records to {destination}",
                IsSuccess = true
            });
        }

        public async Task<bool> CheckSensitiveDataAccessAsync(int userId, string dataType)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            // Define access matrix
            var sensitiveDataTypes = new[] { "MedicalRecord", "Prescription", "LabTest" };
            var authorizedRoles = new[] { 1, 2 }; // Admin, Doctor role IDs

            if (sensitiveDataTypes.Contains(dataType))
            {
                return roles.Any(r => authorizedRoles.Contains(r));
            }

            return true;
        }

        private string RedactValue(string value, DlpScanType type)
        {
            return type switch
            {
                DlpScanType.Email => "***@***.***",
                DlpScanType.PhoneNumber => "***-***-****",
                DlpScanType.IdentityCard => "***-***-***",
                DlpScanType.CreditCard => "****-****-****-****",
                _ => "***REDACTED***"
            };
        }
    }

    public async Task<(bool IsBlocked, string ModifiedContent)> ScanAndApplyPolicyAsync(string content, string channel, string contextPath)
{
    var rules = await _context.DlpRules.Where(r => r.IsActive).ToListAsync();
    var modifiedContent = content;
    var isBlocked = false;

    foreach (var rule in rules)
    {
        var matches = Regex.Matches(content, rule.Pattern);
        if (matches.Count > 0)
        {
            var incident = new DlpIncident
            {
                RuleId = rule.Id,
                // ... (lấy userId từ context)
                Channel = channel,
                Context = contextPath,
                MatchedContent = matches[0].Value,
                DetectedAt = DateTime.UtcNow
            };

            switch (rule.Action)
            {
                case "Block":
                    incident.ActionTaken = "Blocked";
                    _context.DlpIncidents.Add(incident);
                    await _context.SaveChangesAsync();
                    return (true, string.Empty);

                case "Redact":
                    incident.ActionTaken = "Redacted";
                    modifiedContent = Regex.Replace(modifiedContent, rule.Pattern, "***REDACTED***");
                    break;

                case "Alert":
                default:
                    incident.ActionTaken = "Alerted";
                    break;
            }
            _context.DlpIncidents.Add(incident);
        }
    }
    await _context.SaveChangesAsync();
    return (isBlocked, modifiedContent);
}
}