// ThreatHunting.cs
namespace EMRSystem.Core.Entities
{
    public class ThreatHuntingQuery
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public string QueryDefinition { get; set; } // JSON
        
        [StringLength(50)]
        public string Severity { get; set; }
        
        public bool IsActive { get; set; }
        
        public int CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastExecutedAt { get; set; }
        public int ExecutionCount { get; set; }
        
        public ApplicationUser CreatedBy { get; set; }
    }
    
    public class ThreatHuntingResult
    {
        public long Id { get; set; }
        
        [Required]
        public int QueryId { get; set; }
        
        public DateTime ExecutionTime { get; set; }
        
        public int MatchCount { get; set; }
        
        public string Results { get; set; } // JSON
        
        public bool HasThreats { get; set; }
        
        public string Summary { get; set; }
        
        public ThreatHuntingQuery Query { get; set; }
    }
    
    public class ThreatIndicator
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string IndicatorType { get; set; } // IP, Domain, Hash, Pattern
        
        [Required]
        [StringLength(500)]
        public string Value { get; set; }
        
        public string Description { get; set; }
        
        [StringLength(50)]
        public string Severity { get; set; }
        
        public string Source { get; set; }
        
        public DateTime AddedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        
        public bool IsActive { get; set; }
        
        public int MatchCount { get; set; }
        public DateTime? LastMatchedAt { get; set; }
    }
    
    public class ThreatHuntingSchedule
    {
        public int Id { get; set; }
        
        [Required]
        public int QueryId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Schedule { get; set; } // Cron expression
        
        public bool IsEnabled { get; set; }
        
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        
        public ThreatHuntingQuery Query { get; set; }
    }
}