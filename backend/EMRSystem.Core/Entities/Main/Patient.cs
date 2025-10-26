// Patient.cs
using System.ComponentModel.DataAnnotations;

namespace EMRSystem.Core.Entities
{
    public class Patient
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        
        [Required]
        public DateTime DateOfBirth { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Gender { get; set; } // Nam, Nữ, Khác
        
        [StringLength(12)]
        public string IdentityCard { get; set; }
        
        [Phone]
        public string PhoneNumber { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        public string Address { get; set; }
        
        [StringLength(20)]
        public string BloodType { get; set; }
        
        public string Allergies { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<MedicalRecord> MedicalRecords { get; set; }

        [SensitiveData]
    [StringLength(12)]
    public string IdentityCard { get; set; } // Will be encrypted
    
    [SensitiveData]
    public string Address { get; set; } // Will be encrypted
    
    [SensitiveData]
    public string Allergies { get; set; } // Will be encrypted
    }
}