// SecurityIncident.cs
namespace EMRSystem.Core.Entities
{
    public class SecurityIncident
    {
        public long Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Severity { get; set; } // Critical, High, Medium, Low
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } // New, Investigating, Contained, Resolved, Closed
        
        [Required]
        [StringLength(100)]
        public string Category { get; set; } // Data Breach, Malware, Phishing, DDoS, etc.
        
        public int? AffectedUserId { get; set; }
        public string AffectedResource { get; set; }
        public string IpAddress { get; set; }
        
        public DateTime DetectedAt { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        
        public int? AssignedToUserId { get; set; }
        public string ResponseActions { get; set; } // JSON array
        public string Evidence { get; set; } // JSON
        public string Resolution { get; set; }
        
        public bool IsAutoDetected { get; set; }
        public string DetectionRule { get; set; }
        
        public ApplicationUser AffectedUser { get; set; }
        public ApplicationUser AssignedToUser { get; set; }
        
        public ICollection<IncidentComment> Comments { get; set; }
        public ICollection<IncidentAction> Actions { get; set; }
    }
    
    public class IncidentComment
    {
        public int Id { get; set; }
        
        [Required]
        public long IncidentId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string Comment { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public SecurityIncident Incident { get; set; }
        public ApplicationUser User { get; set; }
    }
    
    public class IncidentAction
    {
        public int Id { get; set; }
        
        [Required]
        public long IncidentId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string ActionType { get; set; }
        
        public string Description { get; set; }
        
        public int PerformedByUserId { get; set; }
        public DateTime PerformedAt { get; set; }
        
        public string Result { get; set; }
        
        public SecurityIncident Incident { get; set; }
        public ApplicationUser PerformedBy { get; set; }
    }
    
    public class IncidentPlaybook
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(100)]
        public string IncidentCategory { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Severity { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public string Steps { get; set; } // JSON array
        
        public string AutomatedActions { get; set; } // JSON
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}