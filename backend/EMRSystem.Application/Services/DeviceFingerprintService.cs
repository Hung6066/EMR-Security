// DeviceFingerprintService.cs
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EMRSystem.Application.Services
{
    public class DeviceFingerprintService : IDeviceFingerprintService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeviceFingerprintService> _logger;

        public DeviceFingerprintService(
            ApplicationDbContext context,
            ILogger<DeviceFingerprintService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DeviceFingerprint> CreateFingerprintAsync(DeviceFingerprintDto dto)
        {
            var fingerprintHash = GenerateFingerprintHash(dto);

            var existing = await _context.DeviceFingerprints
                .FirstOrDefaultAsync(f => f.FingerprintHash == fingerprintHash);

            if (existing != null)
            {
                existing.LastSeenAt = DateTime.Now;
                existing.VisitCount++;
                await _context.SaveChangesAsync();
                return existing;
            }

            var fingerprint = new DeviceFingerprint
            {
                FingerprintHash = fingerprintHash,
                UserAgent = dto.UserAgent,
                ScreenResolution = dto.ScreenResolution,
                Timezone = dto.Timezone,
                Language = dto.Language,
                Platform = dto.Platform,
                CookiesEnabled = dto.CookiesEnabled,
                Plugins = dto.Plugins != null ? string.Join(",", dto.Plugins) : null,
                CanvasFingerprint = dto.CanvasFingerprint,
                WebGLFingerprint = dto.WebGLFingerprint,
                AudioFingerprint = dto.AudioFingerprint,
                Fonts = dto.Fonts != null ? string.Join(",", dto.Fonts) : null,
                FirstSeenAt = DateTime.Now,
                LastSeenAt = DateTime.Now,
                VisitCount = 1,
                IsTrusted = false,
                RiskScore = 0
            };

            _context.DeviceFingerprints.Add(fingerprint);
            await _context.SaveChangesAsync();

            return fingerprint;
        }

        public async Task<bool> ValidateFingerprintAsync(string fingerprint, int userId)
        {
            var device = await _context.DeviceFingerprints
                .FirstOrDefaultAsync(f => f.FingerprintHash == fingerprint && 
                                         f.UserId == userId &&
                                         f.IsTrusted);

            return device != null;
        }

        public async Task<List<DeviceFingerprint>> GetTrustedDevicesAsync(int userId)
        {
            return await _context.DeviceFingerprints
                .Where(f => f.UserId == userId && f.IsTrusted)
                .OrderByDescending(f => f.LastSeenAt)
                .ToListAsync();
        }

        public async Task TrustDeviceAsync(int userId, string fingerprint)
        {
            var device = await _context.DeviceFingerprints
                .FirstOrDefaultAsync(f => f.FingerprintHash == fingerprint);

            if (device != null)
            {
                device.UserId = userId;
                device.IsTrusted = true;
                device.TrustedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeTrustedDeviceAsync(int deviceId)
        {
            var device = await _context.DeviceFingerprints.FindAsync(deviceId);
            if (device != null)
            {
                device.IsTrusted = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<RiskScore> CalculateRiskScoreAsync(DeviceFingerprintDto dto, int? userId)
        {
            var score = 0;
            var reasons = new List<string>();

            // Check for headless browser
            if (IsHeadlessBrowser(dto.UserAgent))
            {
                score += 50;
                reasons.Add("Headless browser detected");
            }

            // Check for automation tools
            if (HasAutomationSignatures(dto))
            {
                score += 40;
                reasons.Add("Automation tools detected");
            }

            // Check timezone inconsistency
            if (HasTimezoneInconsistency(dto))
            {
                score += 20;
                reasons.Add("Timezone inconsistency");
            }

            // Check for VPN/Proxy
            if (await IsVPNOrProxy(dto))
            {
                score += 30;
                reasons.Add("VPN/Proxy detected");
            }

            // Check device age
            var fingerprintHash = GenerateFingerprintHash(dto);
            var deviceAge = await GetDeviceAge(fingerprintHash);
            if (deviceAge.HasValue && deviceAge.Value.TotalDays < 1)
            {
                score += 25;
                reasons.Add("New device");
            }

            // Check behavioral patterns
            if (userId.HasValue)
            {
                var suspiciousBehavior = await DetectSuspiciousBehavior(userId.Value);
                if (suspiciousBehavior)
                {
                    score += 35;
                    reasons.Add("Suspicious behavioral patterns");
                }
            }

            var level = score switch
            {
                < 30 => "Low",
                < 60 => "Medium",
                _ => "High"
            };

            return new RiskScore
            {
                Score = Math.Min(score, 100),
                Level = level,
                Reasons = reasons,
                RequiresAdditionalVerification = score >= 60
            };
        }

        private string GenerateFingerprintHash(DeviceFingerprintDto dto)
        {
            var components = new[]
            {
                dto.UserAgent ?? "",
                dto.ScreenResolution ?? "",
                dto.Timezone ?? "",
                dto.Language ?? "",
                dto.Platform ?? "",
                dto.CookiesEnabled.ToString(),
                string.Join(",", dto.Plugins ?? new List<string>()),
                dto.CanvasFingerprint ?? "",
                dto.WebGLFingerprint ?? "",
                dto.AudioFingerprint ?? "",
                string.Join(",", dto.Fonts ?? new List<string>())
            };

            var combined = string.Join("|", components);
            
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(combined);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool IsHeadlessBrowser(string userAgent)
        {
            var headlessSignatures = new[]
            {
                "HeadlessChrome",
                "PhantomJS",
                "Selenium",
                "WebDriver"
            };

            return headlessSignatures.Any(sig => 
                userAgent?.Contains(sig, StringComparison.OrdinalIgnoreCase) == true);
        }

        private bool HasAutomationSignatures(DeviceFingerprintDto dto)
        {
            // Check for common automation tool signatures
            return !dto.CookiesEnabled || 
                   dto.Plugins?.Count == 0 ||
                   string.IsNullOrEmpty(dto.CanvasFingerprint);
        }

        private bool HasTimezoneInconsistency(DeviceFingerprintDto dto)
        {
            // Check if timezone matches language/location
            // This is a simplified check
            return false;
        }

        private async Task<bool> IsVPNOrProxy(DeviceFingerprintDto dto)
        {
            // Integrate with VPN detection service
            return false;
        }

        private async Task<TimeSpan?> GetDeviceAge(string fingerprintHash)
        {
            var device = await _context.DeviceFingerprints
                .FirstOrDefaultAsync(f => f.FingerprintHash == fingerprintHash);

            return device != null 
                ? DateTime.Now - device.FirstSeenAt 
                : null;
        }

        private async Task<bool> DetectSuspiciousBehavior(int userId)
        {
            // Check for suspicious patterns:
            // - Multiple failed logins
            // - Rapid succession of actions
            // - Unusual access patterns
            
            var recentAttempts = await _context.LoginAttempts
                .Where(a => a.UserId == userId && 
                           a.AttemptedAt > DateTime.Now.AddHours(-1))
                .CountAsync();

            return recentAttempts > 10;
        }
    }
}