// TwoFactorAuth.cs
namespace EMRSystem.Core.Entities
{
    public class TwoFactorAuth
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string SecretKey { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? EnabledAt { get; set; }
        
        public ApplicationUser User { get; set; }
    }
    
    public class TwoFactorBackupCode
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Code { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UsedAt { get; set; }
        
        public ApplicationUser User { get; set; }
    }
    
    public class LoginAttempt
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Email { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public bool IsSuccessful { get; set; }
        public string FailureReason { get; set; }
        public DateTime AttemptedAt { get; set; }
        public string Location { get; set; }
        
        public ApplicationUser User { get; set; }
    }
    
    public class UserSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string SessionToken { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string DeviceInfo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public bool IsActive { get; set; }
        
        public ApplicationUser User { get; set; }
    }
}