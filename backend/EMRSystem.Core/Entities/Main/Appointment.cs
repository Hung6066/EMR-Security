// Appointment.cs
namespace EMRSystem.Core.Entities
{
    public class Appointment
    {
        public int Id { get; set; }
        
        [Required]
        public int PatientId { get; set; }
        
        [Required]
        public int DoctorId { get; set; }
        
        [Required]
        public DateTime AppointmentDate { get; set; }
        
        [Required]
        public TimeSpan AppointmentTime { get; set; }
        
        public int DurationMinutes { get; set; } = 30;
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } // Pending, Confirmed, Cancelled, Completed
        
        [StringLength(500)]
        public string Reason { get; set; }
        
        public string Notes { get; set; }
        
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; set; }
        
        // Navigation properties
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public ApplicationUser CreatedByUser { get; set; }
    }
}