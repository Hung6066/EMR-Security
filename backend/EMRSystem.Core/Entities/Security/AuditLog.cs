// AuditLog.cs
namespace EMRSystem.Core.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }
        
        [Required]
        public int? UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Action { get; set; } // CREATE, READ, UPDATE, DELETE
        
        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } // Patient, MedicalRecord, etc.
        
        public int? EntityId { get; set; }
        
        public string OldValues { get; set; } // JSON
        public string NewValues { get; set; } // JSON
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        [Required]
        [StringLength(50)]
        public string IpAddress { get; set; }
        
        [StringLength(500)]
        public string UserAgent { get; set; }
        
        public string AdditionalInfo { get; set; }
        
        // Compliance fields
        public bool IsSuccess { get; set; }
        public string FailureReason { get; set; }
        public string ComplianceNotes { get; set; }
        
        public ApplicationUser User { get; set; }
    }
}