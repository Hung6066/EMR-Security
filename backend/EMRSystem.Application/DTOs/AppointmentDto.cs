// AppointmentDto.cs
namespace EMRSystem.Application.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string Notes { get; set; }
    }

    public class CreateAppointmentDto
    {
        [Required]
        public int PatientId { get; set; }
        
        [Required]
        public int DoctorId { get; set; }
        
        [Required]
        public DateTime AppointmentDate { get; set; }
        
        [Required]
        public TimeSpan AppointmentTime { get; set; }
        
        public int DurationMinutes { get; set; } = 30;
        
        [StringLength(500)]
        public string Reason { get; set; }
        
        public string Notes { get; set; }
    }

    public class UpdateAppointmentStatusDto
    {
        [Required]
        public string Status { get; set; }
        
        public string Notes { get; set; }
    }
}