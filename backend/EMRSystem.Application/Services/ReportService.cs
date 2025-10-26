// ReportService.cs
namespace EMRSystem.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            return new DashboardStatsDto
            {
                TotalPatients = await _context.Patients.CountAsync(),
                TotalAppointmentsToday = await _context.Appointments
                    .CountAsync(a => a.AppointmentDate.Date == today),
                PendingAppointments = await _context.Appointments
                    .CountAsync(a => a.Status == "Pending"),
                CompletedAppointmentsThisMonth = await _context.Appointments
                    .CountAsync(a => a.Status == "Completed" && 
                                    a.AppointmentDate >= firstDayOfMonth),
                NewPatientsThisMonth = await _context.Patients
                    .CountAsync(p => p.CreatedAt >= firstDayOfMonth)
            };
        }

        public async Task<PatientStatisticsDto> GetPatientStatisticsAsync()
        {
            var patients = await _context.Patients.ToListAsync();

            var byGender = patients
                .GroupBy(p => p.Gender)
                .ToDictionary(g => g.Key, g => g.Count());

            var byAgeGroup = patients
                .GroupBy(p => GetAgeGroup(p.DateOfBirth))
                .ToDictionary(g => g.Key, g => g.Count());

            var byBloodType = patients
                .Where(p => !string.IsNullOrEmpty(p.BloodType))
                .GroupBy(p => p.BloodType)
                .ToDictionary(g => g.Key, g => g.Count());

            return new PatientStatisticsDto
            {
                PatientsByGender = byGender,
                PatientsByAgeGroup = byAgeGroup,
                PatientsByBloodType = byBloodType
            };
        }

        public async Task<AppointmentStatisticsDto> GetAppointmentStatisticsAsync(
            DateTime startDate, 
            DateTime endDate)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                .ToListAsync();

            var byStatus = appointments
                .GroupBy(a => a.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var byDoctor = appointments
                .GroupBy(a => a.Doctor.FullName)
                .ToDictionary(g => g.Key, g => g.Count());

            var byDay = appointments
                .GroupBy(a => a.AppointmentDate.Date)
                .Select(g => new DailyAppointmentCount
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            return new AppointmentStatisticsDto
            {
                AppointmentsByStatus = byStatus,
                AppointmentsByDoctor = byDoctor,
                AppointmentsByDay = byDay
            };
        }

        public async Task<MedicalRecordStatisticsDto> GetMedicalRecordStatisticsAsync(
            DateTime startDate, 
            DateTime endDate)
        {
            var records = await _context.MedicalRecords
                .Where(m => m.VisitDate >= startDate && m.VisitDate <= endDate)
                .ToListAsync();

            var byDiagnosis = records
                .Where(r => !string.IsNullOrEmpty(r.Diagnosis))
                .GroupBy(r => r.Diagnosis)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count());

            var byMonth = records
                .GroupBy(r => r.VisitDate.ToString("yyyy-MM"))
                .ToDictionary(g => g.Key, g => g.Count());

            return new MedicalRecordStatisticsDto
            {
                TotalRecords = records.Count,
                RecordsByDiagnosis = byDiagnosis,
                RecordsByMonth = byMonth
            };
        }

        private string GetAgeGroup(DateTime dateOfBirth)
        {
            var age = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > DateTime.Today.AddYears(-age)) age--;

            if (age < 18) return "0-17";
            if (age < 30) return "18-29";
            if (age < 45) return "30-44";
            if (age < 60) return "45-59";
            return "60+";
        }
    }
}