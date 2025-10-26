// IAppointmentService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto, int userId);
        Task<AppointmentDto> GetByIdAsync(int id);
        Task<IEnumerable<AppointmentDto>> GetByPatientIdAsync(int patientId);
        Task<IEnumerable<AppointmentDto>> GetByDoctorIdAsync(int doctorId, DateTime date);
        Task<IEnumerable<AppointmentDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto);
        Task CancelAppointmentAsync(int id, string reason);
        Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime date, TimeSpan time);
    }
}