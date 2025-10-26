// MedicalRecord.cs
namespace EMRSystem.Core.Entities
{
    public class MedicalRecord
    {
        public int Id { get; set; }
        
        [Required]
        public int PatientId { get; set; }
        
        [Required]
        public int DoctorId { get; set; }
        
        [Required]
        public DateTime VisitDate { get; set; }
        
        [Required]
        public string ChiefComplaint { get; set; } // Lý do khám
        
        public string PresentIllness { get; set; } // Bệnh sử
        
        public string PhysicalExamination { get; set; } // Khám lâm sàng
        
        public string Diagnosis { get; set; } // Chẩn đoán
        
        public string Treatment { get; set; } // Điều trị
        
        public string Notes { get; set; }
        
        // Vital Signs
        public decimal? Temperature { get; set; }
        public string BloodPressure { get; set; }
        public int? HeartRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public ICollection<Prescription> Prescriptions { get; set; }
        public ICollection<LabTest> LabTests { get; set; }
    }
}