// ThreatIntelligence.cs
namespace EMRSystem.Core.Entities
{
    public class IpBlacklist
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string IpAddress { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Reason { get; set; }
        
        public DateTime BlockedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        
        public int BlockedBy { get; set; }
        public bool IsActive { get; set; }
        
        public ApplicationUser BlockedByUser { get; set; }
    }
    
    public class ThreatLog
    {
        public long Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string IpAddress { get; set; }
        
        [Required]
        [StringLength(100)]
        public string ThreatType { get; set; }
        
        public int Severity { get; set; }
        
        public string Description { get; set; }
        public string Metadata { get; set; }
        
        public DateTime DetectedAt { get; set; }
        
        public bool IsBlocked { get; set; }
        public string Action { get; set; }
    }
}