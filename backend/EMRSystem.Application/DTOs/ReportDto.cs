// ReportDto.cs
namespace EMRSystem.Application.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalPatients { get; set; }
        public int TotalAppointmentsToday { get; set; }
        public int PendingAppointments { get; set; }
        public int CompletedAppointmentsThisMonth { get; set; }
        public int NewPatientsThisMonth { get; set; }
    }

    public class PatientStatisticsDto
    {
        public Dictionary<string, int> PatientsByGender { get; set; }
        public Dictionary<string, int> PatientsByAgeGroup { get; set; }
        public Dictionary<string, int> PatientsByBloodType { get; set; }
    }

    public class AppointmentStatisticsDto
    {
        public Dictionary<string, int> AppointmentsByStatus { get; set; }
        public Dictionary<string, int> AppointmentsByDoctor { get; set; }
        public List<DailyAppointmentCount> AppointmentsByDay { get; set; }
    }

    public class DailyAppointmentCount
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class MedicalRecordStatisticsDto
    {
        public int TotalRecords { get; set; }
        public Dictionary<string, int> RecordsByDiagnosis { get; set; }
        public Dictionary<string, int> RecordsByMonth { get; set; }
    }
}