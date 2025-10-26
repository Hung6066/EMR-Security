// ApiKey.cs
namespace EMRSystem.Core.Entities
{
    public class ApiKey
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(64)]
        public string KeyHash { get; set; }
        
        [Required]
        [StringLength(10)]
        public string KeyPrefix { get; set; } // First 10 chars for identification
        
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        
        public bool IsActive { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        
        public string IpWhitelist { get; set; } // Comma-separated IPs
        public string AllowedScopes { get; set; } // Comma-separated scopes
        
        public int? RateLimitPerMinute { get; set; }
        
        public ApplicationUser User { get; set; }
    }
}