// PatientRepository.cs
using Microsoft.EntityFrameworkCore;
using EMRSystem.Core.Entities;
using EMRSystem.Infrastructure.Data;

namespace EMRSystem.Infrastructure.Repositories
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Task<Patient> GetPatientWithRecordsAsync(int id);
        Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm);
    }

    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        public PatientRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Patient> GetPatientWithRecordsAsync(int id)
        {
            return await _context.Patients
                .Include(p => p.MedicalRecords)
                    .ThenInclude(m => m.Doctor)
                .Include(p => p.MedicalRecords)
                    .ThenInclude(m => m.Prescriptions)
                        .ThenInclude(pr => pr.PrescriptionDetails)
                .Include(p => p.MedicalRecords)
                    .ThenInclude(m => m.LabTests)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm)
        {
            return await _context.Patients
                .Where(p => p.FullName.Contains(searchTerm) ||
                           p.IdentityCard.Contains(searchTerm) ||
                           p.PhoneNumber.Contains(searchTerm))
                .ToListAsync();
        }
    }
}