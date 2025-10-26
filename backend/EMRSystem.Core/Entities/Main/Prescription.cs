// Prescription.cs
namespace EMRSystem.Core.Entities
{
    public class Prescription
    {
        public int Id { get; set; }
        
        [Required]
        public int MedicalRecordId { get; set; }
        
        [Required]
        public DateTime PrescriptionDate { get; set; }
        
        public string Notes { get; set; }
        
        // Navigation properties
        public MedicalRecord MedicalRecord { get; set; }
        public ICollection<PrescriptionDetail> PrescriptionDetails { get; set; }
    }
    
    public class PrescriptionDetail
    {
        public int Id { get; set; }
        
        [Required]
        public int PrescriptionId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string MedicineName { get; set; }
        
        [Required]
        public string Dosage { get; set; } // Liều lượng
        
        [Required]
        public string Frequency { get; set; } // Tần suất
        
        [Required]
        public int Duration { get; set; } // Số ngày
        
        public string Instructions { get; set; } // Hướng dẫn sử dụng
        
        // Navigation properties
        public Prescription Prescription { get; set; }
    }
}