// Doctor.cs
namespace EMRSystem.Core.Entities
{
    public class Doctor
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Specialty { get; set; } // Chuyên khoa
        
        [Required]
        [StringLength(20)]
        public string LicenseNumber { get; set; }
        
        [Phone]
        public string PhoneNumber { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public ICollection<MedicalRecord> MedicalRecords { get; set; }
    }
}