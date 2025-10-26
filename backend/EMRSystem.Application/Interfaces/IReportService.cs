// IReportService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IReportService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<PatientStatisticsDto> GetPatientStatisticsAsync();
        Task<AppointmentStatisticsDto> GetAppointmentStatisticsAsync(DateTime startDate, DateTime endDate);
        Task<MedicalRecordStatisticsDto> GetMedicalRecordStatisticsAsync(DateTime startDate, DateTime endDate);
    }
}