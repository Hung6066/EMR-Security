// ZeroTrustPolicy.cs
namespace EMRSystem.Core.Entities
{
    public class ZeroTrustPolicy
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public string ResourceType { get; set; } // API, Page, Data
        
        [Required]
        public string ResourcePath { get; set; }
        
        public string AllowedRoles { get; set; }
        public string RequiredConditions { get; set; } // JSON
        
        public int MinTrustScore { get; set; }
        public bool RequiresMFA { get; set; }
        public bool RequiresDeviceCompliance { get; set; }
        public bool RequiresNetworkCompliance { get; set; }
        
        public int Priority { get; set; }
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class AccessDecision
    {
        public long Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Resource { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Action { get; set; }
        
        public bool IsAllowed { get; set; }
        
        public int TrustScore { get; set; }
        
        public string DenialReason { get; set; }
        public string PolicyApplied { get; set; }
        public string Context { get; set; } // JSON
        
        public DateTime DecisionTime { get; set; }
        
        public ApplicationUser User { get; set; }
    }
    
    public class TrustScore
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        public int DeviceScore { get; set; }
        public int LocationScore { get; set; }
        public int BehaviorScore { get; set; }
        public int TimeScore { get; set; }
        public int NetworkScore { get; set; }
        
        public int OverallScore { get; set; }
        
        public DateTime CalculatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        
        public string CalculationDetails { get; set; } // JSON
        
        public ApplicationUser User { get; set; }
    }
}