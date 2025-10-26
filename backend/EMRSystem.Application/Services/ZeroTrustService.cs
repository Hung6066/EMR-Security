// ZeroTrustService.cs
using System.Text.Json;

namespace EMRSystem.Application.Services
{
    public class ZeroTrustService : IZeroTrustService
    {
        private readonly ApplicationDbContext _context;
        private readonly IDeviceFingerprintService _deviceService;
        private readonly IThreatIntelligenceService _threatService;
        private readonly ILogger<ZeroTrustService> _logger;

        public ZeroTrustService(
            ApplicationDbContext context,
            IDeviceFingerprintService deviceService,
            IThreatIntelligenceService threatService,
            ILogger<ZeroTrustService> logger)
        {
            _context = context;
            _deviceService = deviceService;
            _threatService = threatService;
            _logger = logger;
        }

        public async Task<AccessDecisionResult> EvaluateAccessAsync(AccessRequest request)
        {
            var result = new AccessDecisionResult
            {
                IsAllowed = true,
                DenialReasons = new List<string>(),
                AppliedPolicies = new List<string>(),
                RequiredActions = new Dictionary<string, object>()
            };

            // Calculate trust score
            var trustScore = await CalculateTrustScoreAsync(request.UserId, request.Context);
            result.TrustScore = trustScore.OverallScore;

            // Get applicable policies
            var policies = await GetApplicablePoliciesAsync(
                ExtractResourceType(request.Resource),
                request.Resource
            );

            // Evaluate each policy
            foreach (var policy in policies.OrderByDescending(p => p.Priority))
            {
                if (!policy.IsActive) continue;

                var policyResult = await EvaluatePolicyAsync(policy, request, trustScore);
                
                if (!policyResult.IsAllowed)
                {
                    result.IsAllowed = false;
                    result.DenialReasons.AddRange(policyResult.Reasons);
                }
                
                result.AppliedPolicies.Add(policy.Name);

                // If high priority policy denies, stop evaluation
                if (!policyResult.IsAllowed && policy.Priority >= 90)
                {
                    break;
                }
            }

            // Check minimum requirements
            if (result.IsAllowed)
            {
                var complianceCheck = await CheckMinimumComplianceAsync(request, trustScore);
                if (!complianceCheck.IsCompliant)
                {
                    result.IsAllowed = false;
                    result.DenialReasons.AddRange(complianceCheck.Reasons);
                }
            }

            // Log decision
            await LogAccessDecisionAsync(result);

            return result;
        }

        public async Task<TrustScoreResult> CalculateTrustScoreAsync(int userId, AccessContext context)
        {
            var result = new TrustScoreResult
            {
                Details = new Dictionary<string, string>()
            };

            // Device Score (0-100)
            result.DeviceScore = await CalculateDeviceScoreAsync(context.DeviceFingerprint, userId);
            result.Details["device"] = $"Device trust: {result.DeviceScore}/100";

            // Location Score (0-100)
            result.LocationScore = await CalculateLocationScoreAsync(context.IpAddress, userId);
            result.Details["location"] = $"Location trust: {result.LocationScore}/100";

            // Behavior Score (0-100)
            result.BehaviorScore = await CalculateBehaviorScoreAsync(userId, context);
            result.Details["behavior"] = $"Behavior pattern: {result.BehaviorScore}/100";

            // Time Score (0-100)
            result.TimeScore = CalculateTimeScore(context.Timestamp, userId);
            result.Details["time"] = $"Time pattern: {result.TimeScore}/100";

            // Network Score (0-100)
            result.NetworkScore = await CalculateNetworkScoreAsync(context.IpAddress);
            result.Details["network"] = $"Network security: {result.NetworkScore}/100";

            // MFA Bonus
            int mfaBonus = context.HasMFA ? 20 : 0;
            result.Details["mfa"] = context.HasMFA ? "MFA enabled (+20)" : "MFA not used";

            // Calculate overall score (weighted average)
            result.OverallScore = (int)(
                (result.DeviceScore * 0.25) +
                (result.LocationScore * 0.20) +
                (result.BehaviorScore * 0.20) +
                (result.TimeScore * 0.15) +
                (result.NetworkScore * 0.20) +
                mfaBonus
            );

            // Cap at 100
            result.OverallScore = Math.Min(result.OverallScore, 100);

            // Save trust score
            await SaveTrustScoreAsync(userId, result);

            return result;
        }

        public async Task<List<ZeroTrustPolicy>> GetApplicablePoliciesAsync(
            string resourceType, 
            string resourcePath)
        {
            return await _context.ZeroTrustPolicies
                .Where(p => p.IsActive && 
                           p.ResourceType == resourceType &&
                           (p.ResourcePath == "*" || resourcePath.StartsWith(p.ResourcePath)))
                .OrderByDescending(p => p.Priority)
                .ToListAsync();
        }

        public async Task<bool> VerifyDeviceComplianceAsync(string deviceFingerprint)
        {
            var device = await _context.DeviceFingerprints
                .FirstOrDefaultAsync(d => d.FingerprintHash == deviceFingerprint);

            if (device == null) return false;

            // Check if device is trusted
            if (!device.IsTrusted) return false;

            // Check device age
            var deviceAge = DateTime.Now - device.FirstSeenAt;
            if (deviceAge.TotalDays < 1) return false; // Too new

            // Check risk score
            if (device.RiskScore > 50) return false;

            return true;
        }

        public async Task<bool> VerifyNetworkComplianceAsync(string ipAddress)
        {
            // Check if IP is blacklisted
            var isBlacklisted = await _threatService.IsIpBlacklistedAsync(ipAddress);
            if (isBlacklisted) return false;

            // Check threat assessment
            var assessment = await _threatService.AssessIpAddressAsync(ipAddress);
            if (assessment.IsThreat) return false;

            // Check if using VPN/Proxy (depending on policy)
            if (assessment.IsVPN || assessment.IsProxy || assessment.IsTor)
            {
                // Can be configured per policy
                return false;
            }

            return true;
        }

        public async Task LogAccessDecisionAsync(AccessDecisionResult decision)
        {
            // Implementation for logging access decisions
        }

        private async Task<(bool IsAllowed, List<string> Reasons)> EvaluatePolicyAsync(
            ZeroTrustPolicy policy,
            AccessRequest request,
            TrustScoreResult trustScore)
        {
            var reasons = new List<string>();
            var isAllowed = true;

            // Check trust score
            if (trustScore.OverallScore < policy.MinTrustScore)
            {
                isAllowed = false;
                reasons.Add($"Trust score {trustScore.OverallScore} below required {policy.MinTrustScore}");
            }

            // Check MFA requirement
            if (policy.RequiresMFA && !request.Context.HasMFA)
            {
                isAllowed = false;
                reasons.Add("MFA required but not provided");
            }

            // Check device compliance
            if (policy.RequiresDeviceCompliance)
            {
                var isCompliant = await VerifyDeviceComplianceAsync(request.Context.DeviceFingerprint);
                if (!isCompliant)
                {
                    isAllowed = false;
                    reasons.Add("Device not compliant");
                }
            }

            // Check network compliance
            if (policy.RequiresNetworkCompliance)
            {
                var isCompliant = await VerifyNetworkComplianceAsync(request.Context.IpAddress);
                if (!isCompliant)
                {
                    isAllowed = false;
                    reasons.Add("Network not compliant");
                }
            }

            // Check role-based access
            if (!string.IsNullOrEmpty(policy.AllowedRoles))
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == request.UserId);

                var userRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
                var allowedRoles = policy.AllowedRoles.Split(',').Select(r => r.Trim());

                if (!userRoles.Any(r => allowedRoles.Contains(r)))
                {
                    isAllowed = false;
                    reasons.Add("User role not authorized");
                }
            }

            return (isAllowed, reasons);
        }

        private async Task<(bool IsCompliant, List<string> Reasons)> CheckMinimumComplianceAsync(
            AccessRequest request,
            TrustScoreResult trustScore)
        {
            var reasons = new List<string>();
            var isCompliant = true;

            // Minimum trust score threshold
            if (trustScore.OverallScore < 30)
            {
                isCompliant = false;
                reasons.Add("Trust score too low");
            }

            // Device must be known
            if (string.IsNullOrEmpty(request.Context.DeviceFingerprint))
            {
                isCompliant = false;
                reasons.Add("Unknown device");
            }

            return (isCompliant, reasons);
        }

        private async Task<int> CalculateDeviceScoreAsync(string deviceFingerprint, int userId)
        {
            if (string.IsNullOrEmpty(deviceFingerprint)) return 0;

            var device = await _context.DeviceFingerprints
                .FirstOrDefaultAsync(d => d.FingerprintHash == deviceFingerprint);

            if (device == null) return 30; // Unknown device

            int score = 50;

            // Trusted device bonus
            if (device.IsTrusted) score += 30;

            // Device age factor
            var deviceAge = DateTime.Now - device.FirstSeenAt;
            if (deviceAge.TotalDays > 30) score += 10;
            if (deviceAge.TotalDays > 90) score += 10;

            // Usage frequency
            if (device.VisitCount > 100) score += 10;

            // Deduct for risk
            score -= device.RiskScore / 2;

            return Math.Max(0, Math.Min(100, score));
        }

        private async Task<int> CalculateLocationScoreAsync(string ipAddress, int userId)
        {
            var recentLogins = await _context.LoginAttempts
                .Where(a => a.UserId == userId && 
                           a.IsSuccessful &&
                           a.AttemptedAt > DateTime.Now.AddDays(-30))
                .Select(a => a.IpAddress)
                .Distinct()
                .ToListAsync();

            int score = 50;

            // Known location bonus
            if (recentLogins.Contains(ipAddress))
            {
                score += 30;
            }

            // Threat assessment
            var threat = await _threatService.AssessIpAddressAsync(ipAddress);
            if (threat.IsThreat)
            {
                score -= 40;
            }

            if (threat.IsVPN || threat.IsProxy)
            {
                score -= 20;
            }

            return Math.Max(0, Math.Min(100, score));
        }

        private async Task<int> CalculateBehaviorScoreAsync(int userId, AccessContext context)
        {
            // Analyze recent user behavior patterns
            var recentActivity = await _context.AuditLogs
                .Where(a => a.UserId == userId && 
                           a.Timestamp > DateTime.Now.AddHours(-24))
                .ToListAsync();

            int score = 70;

            // Check for unusual activity volume
            if (recentActivity.Count > 1000) score -= 30; // Too many actions

            // Check for failed attempts
            var failedAttempts = recentActivity.Where(a => !a.IsSuccess).Count();
            if (failedAttempts > 10) score -= 20;

            return Math.Max(0, Math.Min(100, score));
        }

        private int CalculateTimeScore(DateTime timestamp, int userId)
        {
            var hour = timestamp.Hour;
            
            int score = 70;

            // Business hours bonus (8 AM - 6 PM)
            if (hour >= 8 && hour <= 18)
            {
                score += 20;
            }
            // Odd hours penalty
            else if (hour >= 0 && hour <= 5)
            {
                score -= 30;
            }

            // Weekend check
            if (timestamp.DayOfWeek == DayOfWeek.Saturday || 
                timestamp.DayOfWeek == DayOfWeek.Sunday)
            {
                score -= 10;
            }

            return Math.Max(0, Math.Min(100, score));
        }

        private async Task<int> CalculateNetworkScoreAsync(string ipAddress)
        {
            var assessment = await _threatService.AssessIpAddressAsync(ipAddress);
            
            int score = 100 - assessment.ThreatScore;

            return Math.Max(0, Math.Min(100, score));
        }

        private async Task SaveTrustScoreAsync(int userId, TrustScoreResult result)
        {
            var trustScore = new TrustScore
            {
                UserId = userId,
                DeviceScore = result.DeviceScore,
                LocationScore = result.LocationScore,
                BehaviorScore = result.BehaviorScore,
                TimeScore = result.TimeScore,
                NetworkScore = result.NetworkScore,
                OverallScore = result.OverallScore,
                CalculatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddMinutes(15),
                CalculationDetails = JsonSerializer.Serialize(result.Details)
            };

            _context.TrustScores.Add(trustScore);
            await _context.SaveChangesAsync();
        }

        private string ExtractResourceType(string resource)
        {
            if (resource.StartsWith("/api/")) return "API";
            if (resource.StartsWith("/data/")) return "Data";
            return "Page";
        }
    }
}