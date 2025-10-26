// AppointmentService.cs
namespace EMRSystem.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AppointmentService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AppointmentDto> CreateAppointmentAsync(CreateAppointmentDto dto, int userId)
        {
            // Check if time slot is available
            var isAvailable = await IsTimeSlotAvailableAsync(dto.DoctorId, dto.AppointmentDate, dto.AppointmentTime);
            if (!isAvailable)
                throw new Exception("Time slot is not available");

            var appointment = _mapper.Map<Appointment>(dto);
            appointment.Status = "Pending";
            appointment.CreatedBy = userId;
            appointment.CreatedAt = DateTime.Now;

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(appointment.Id);
        }

        public async Task<AppointmentDto> GetByIdAsync(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                throw new Exception("Appointment not found");

            return _mapper.Map<AppointmentDto>(appointment);
        }

        public async Task<IEnumerable<AppointmentDto>> GetByPatientIdAsync(int patientId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<IEnumerable<AppointmentDto>> GetByDoctorIdAsync(int doctorId, DateTime date)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
                .OrderBy(a => a.AppointmentTime)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task<IEnumerable<AppointmentDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentTime)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
        }

        public async Task UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                throw new Exception("Appointment not found");

            appointment.Status = dto.Status;
            appointment.Notes = dto.Notes;

            if (dto.Status == "Confirmed")
                appointment.ConfirmedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task CancelAppointmentAsync(int id, string reason)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                throw new Exception("Appointment not found");

            appointment.Status = "Cancelled";
            appointment.CancelledAt = DateTime.Now;
            appointment.CancellationReason = reason;

            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsTimeSlotAvailableAsync(int doctorId, DateTime date, TimeSpan time)
        {
            var existingAppointment = await _context.Appointments
                .FirstOrDefaultAsync(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDate.Date == date.Date &&
                    a.AppointmentTime == time &&
                    a.Status != "Cancelled");

            return existingAppointment == null;
        }
    }
}