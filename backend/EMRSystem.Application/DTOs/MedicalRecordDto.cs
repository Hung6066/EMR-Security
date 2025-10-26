// MedicalRecordDto.cs
namespace EMRSystem.Application.DTOs
{
    public class MedicalRecordDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public DateTime VisitDate { get; set; }
        public string ChiefComplaint { get; set; }
        public string PresentIllness { get; set; }
        public string PhysicalExamination { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }
        public VitalSignsDto VitalSigns { get; set; }
    }

    public class VitalSignsDto
    {
        public decimal? Temperature { get; set; }
        public string BloodPressure { get; set; }
        public int? HeartRate { get; set; }
        public int? RespiratoryRate { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
    }

    public class CreateMedicalRecordDto
    {
        [Required]
        public int PatientId { get; set; }
        
        [Required]
        public int DoctorId { get; set; }
        
        [Required]
        public DateTime VisitDate { get; set; }
        
        [Required]
        public string ChiefComplaint { get; set; }
        
        public string PresentIllness { get; set; }
        public string PhysicalExamination { get; set; }
        public string Diagnosis { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }
        public VitalSignsDto VitalSigns { get; set; }
    }
}