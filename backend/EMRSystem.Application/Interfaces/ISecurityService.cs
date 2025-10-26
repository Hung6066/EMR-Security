// ISecurityService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface ISecurityService
    {
        Task LogLoginAttemptAsync(string email, string ipAddress, string userAgent, bool isSuccessful, string failureReason = null);
        Task<bool> IsAccountLockedAsync(string email);
        Task CreateSessionAsync(int userId, string token, string ipAddress, string userAgent, string deviceInfo);
        Task<List<UserSession>> GetActiveSessionsAsync(int userId);
        Task RevokeSessionAsync(int sessionId);
        Task RevokeAllSessionsAsync(int userId, int exceptSessionId = 0);
        Task<bool> CheckSuspiciousActivityAsync(int userId, string ipAddress);
        Task SendSecurityAlertAsync(int userId, string alertType, string details);
    }
}