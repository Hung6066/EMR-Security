// PatientService.cs
using AutoMapper;
using EMRSystem.Application.DTOs;
using EMRSystem.Core.Entities;
using EMRSystem.Infrastructure.Repositories;

namespace EMRSystem.Application.Services
{
    public interface IPatientService
    {
        Task<IEnumerable<PatientDto>> GetAllPatientsAsync();
        Task<PatientDto> GetPatientByIdAsync(int id);
        Task<PatientDto> CreatePatientAsync(CreatePatientDto dto);
        Task UpdatePatientAsync(UpdatePatientDto dto);
        Task DeletePatientAsync(int id);
        Task<IEnumerable<PatientDto>> SearchPatientsAsync(string searchTerm);
    }

    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _repository;
        private readonly IMapper _mapper;

        public PatientService(IPatientRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PatientDto>> GetAllPatientsAsync()
        {
            var patients = await _repository.GetAllAsync();
            return _mapper.Map<IEnumerable<PatientDto>>(patients);
        }

        public async Task<PatientDto> GetPatientByIdAsync(int id)
        {
            var patient = await _repository.GetByIdAsync(id);
            return _mapper.Map<PatientDto>(patient);
        }

        public async Task<PatientDto> CreatePatientAsync(CreatePatientDto dto)
        {
            var patient = _mapper.Map<Patient>(dto);
            patient.CreatedAt = DateTime.Now;
            
            var created = await _repository.AddAsync(patient);
            return _mapper.Map<PatientDto>(created);
        }

        public async Task UpdatePatientAsync(UpdatePatientDto dto)
        {
            var patient = await _repository.GetByIdAsync(dto.Id);
            if (patient == null)
                throw new Exception("Patient not found");

            _mapper.Map(dto, patient);
            patient.UpdatedAt = DateTime.Now;
            
            await _repository.UpdateAsync(patient);
        }

        public async Task DeletePatientAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<PatientDto>> SearchPatientsAsync(string searchTerm)
        {
            var patients = await _repository.SearchPatientsAsync(searchTerm);
            return _mapper.Map<IEnumerable<PatientDto>>(patients);
        }
    }
}