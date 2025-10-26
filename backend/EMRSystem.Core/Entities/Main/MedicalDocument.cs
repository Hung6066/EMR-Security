// MedicalDocument.cs
namespace EMRSystem.Core.Entities
{
    public class MedicalDocument
    {
        public int Id { get; set; }
        
        [Required]
        public int MedicalRecordId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string FileName { get; set; }
        
        [Required]
        [StringLength(500)]
        public string FilePath { get; set; }
        
        [Required]
        [StringLength(50)]
        public string FileType { get; set; } // Image, PDF, Document
        
        [StringLength(100)]
        public string ContentType { get; set; }
        
        public long FileSize { get; set; }
        
        public string Description { get; set; }
        
        public int UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
        
        // Navigation properties
        public MedicalRecord MedicalRecord { get; set; }
        public ApplicationUser UploadedByUser { get; set; }
    }
}