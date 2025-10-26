// LabTest.cs
namespace EMRSystem.Core.Entities
{
    public class LabTest
    {
        public int Id { get; set; }
        
        [Required]
        public int MedicalRecordId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string TestName { get; set; }
        
        [Required]
        public DateTime OrderDate { get; set; }
        
        public DateTime? ResultDate { get; set; }
        
        public string Result { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } // Đang chờ, Hoàn thành
        
        public string Notes { get; set; }
        
        // Navigation properties
        public MedicalRecord MedicalRecord { get; set; }
    }
}