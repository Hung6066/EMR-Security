// SecurityService.cs
namespace EMRSystem.Application.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<SecurityService> _logger;

        public SecurityService(
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<SecurityService> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task LogLoginAttemptAsync(
            string email, 
            string ipAddress, 
            string userAgent, 
            bool isSuccessful, 
            string failureReason = null)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            var attempt = new LoginAttempt
            {
                UserId = user?.Id,
                Email = email,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsSuccessful = isSuccessful,
                FailureReason = failureReason,
                AttemptedAt = DateTime.Now,
                Location = await GetLocationFromIpAsync(ipAddress)
            };

            _context.LoginAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            // Check for suspicious activity
            if (user != null && !isSuccessful)
            {
                await CheckFailedAttemptsAsync(user.Id, email);
            }
        }

        public async Task<bool> IsAccountLockedAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            // Check failed attempts in last 15 minutes
            var recentFailures = await _context.LoginAttempts
                .Where(a => a.Email == email && 
                           !a.IsSuccessful && 
                           a.AttemptedAt > DateTime.Now.AddMinutes(-15))
                .CountAsync();

            return recentFailures >= 5;
        }

        public async Task CreateSessionAsync(
            int userId, 
            string token, 
            string ipAddress, 
            string userAgent, 
            string deviceInfo)
        {
            var session = new UserSession
            {
                UserId = userId,
                SessionToken = token,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                DeviceInfo = deviceInfo,
                CreatedAt = DateTime.Now,
                ExpiresAt = DateTime.Now.AddDays(7),
                LastActivityAt = DateTime.Now,
                IsActive = true
            };

            _context.UserSessions.Add(session);
            await _context.SaveChangesAsync();

            // Check for new device login
            var isNewDevice = !await _context.UserSessions
                .AnyAsync(s => s.UserId == userId && 
                              s.DeviceInfo == deviceInfo && 
                              s.Id != session.Id);

            if (isNewDevice)
            {
                await SendSecurityAlertAsync(userId, "NewDeviceLogin", 
                    $"New login from {deviceInfo} at {ipAddress}");
            }
        }

        public async Task<List<UserSession>> GetActiveSessionsAsync(int userId)
        {
            return await _context.UserSessions
                .Where(s => s.UserId == userId && 
                           s.IsActive && 
                           s.ExpiresAt > DateTime.Now)
                .OrderByDescending(s => s.LastActivityAt)
                .ToListAsync();
        }

        public async Task RevokeSessionAsync(int sessionId)
        {
            var session = await _context.UserSessions.FindAsync(sessionId);
            if (session != null)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RevokeAllSessionsAsync(int userId, int exceptSessionId = 0)
        {
            var sessions = await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive && s.Id != exceptSessionId)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckSuspiciousActivityAsync(int userId, string ipAddress)
        {
            // Check for multiple locations in short time
            var recentLogins = await _context.LoginAttempts
                .Where(a => a.UserId == userId && 
                           a.IsSuccessful && 
                           a.AttemptedAt > DateTime.Now.AddHours(-1))
                .Select(a => a.IpAddress)
                .Distinct()
                .ToListAsync();

            return recentLogins.Count > 3;
        }

        public async Task SendSecurityAlertAsync(int userId, string alertType, string details)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            var subject = alertType switch
            {
                "NewDeviceLogin" => "Đăng nhập từ thiết bị mới",
                "SuspiciousActivity" => "Phát hiện hoạt động đáng ngờ",
                "PasswordChanged" => "Mật khẩu đã được thay đổi",
                _ => "Thông báo bảo mật"
            };

            await _emailService.SendEmailAsync(user.Email, subject, 
                $"Xin chào {user.FullName},<br><br>{details}<br><br>" +
                "Nếu không phải bạn, vui lòng đổi mật khẩu ngay lập tức.");

            _logger.LogWarning($"Security Alert for User {userId}: {alertType} - {details}");
        }

        private async Task CheckFailedAttemptsAsync(int userId, string email)
        {
            var failedCount = await _context.LoginAttempts
                .Where(a => a.Email == email && 
                           !a.IsSuccessful && 
                           a.AttemptedAt > DateTime.Now.AddMinutes(-15))
                .CountAsync();

            if (failedCount == 3)
            {
                await SendSecurityAlertAsync(userId, "FailedLoginAttempts",
                    $"Có {failedCount} lần đăng nhập thất bại vào tài khoản của bạn.");
            }
            else if (failedCount >= 5)
            {
                await SendSecurityAlertAsync(userId, "AccountLocked",
                    "Tài khoản của bạn đã bị khóa do quá nhiều lần đăng nhập thất bại.");
            }
        }

        private async Task<string> GetLocationFromIpAsync(string ipAddress)
        {
            // Implement IP geolocation (using service like IPStack, MaxMind, etc.)
            // For demo, return placeholder
            return "Unknown Location";
        }
    }
}