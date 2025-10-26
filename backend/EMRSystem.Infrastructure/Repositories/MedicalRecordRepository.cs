// MedicalRecordRepository.cs
using Microsoft.EntityFrameworkCore;
using EMRSystem.Core.Entities;
using EMRSystem.Infrastructure.Data;

namespace EMRSystem.Infrastructure.Repositories
{
    public interface IMedicalRecordRepository : IRepository<MedicalRecord>
    {
        Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId);
        Task<MedicalRecord> GetWithDetailsAsync(int id);
    }

    public class MedicalRecordRepository : Repository<MedicalRecord>, IMedicalRecordRepository
    {
        public MedicalRecordRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MedicalRecord>> GetByPatientIdAsync(int patientId)
        {
            return await _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Prescriptions)
                    .ThenInclude(p => p.PrescriptionDetails)
                .Include(m => m.LabTests)
                .Where(m => m.PatientId == patientId)
                .OrderByDescending(m => m.VisitDate)
                .ToListAsync();
        }

        public async Task<MedicalRecord> GetWithDetailsAsync(int id)
        {
            return await _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .Include(m => m.Prescriptions)
                    .ThenInclude(p => p.PrescriptionDetails)
                .Include(m => m.LabTests)
                .FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}