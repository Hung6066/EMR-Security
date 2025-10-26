// MappingProfile.cs
using AutoMapper;
using EMRSystem.Core.Entities;
using EMRSystem.Application.DTOs;

namespace EMRSystem.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Patient mappings
            CreateMap<Patient, PatientDto>();
            CreateMap<CreatePatientDto, Patient>();
            CreateMap<UpdatePatientDto, Patient>();

            // MedicalRecord mappings
            CreateMap<MedicalRecord, MedicalRecordDto>()
                .ForMember(dest => dest.PatientName, opt => opt.MapFrom(src => src.Patient.FullName))
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor.FullName))
                .ForMember(dest => dest.VitalSigns, opt => opt.MapFrom(src => new VitalSignsDto
                {
                    Temperature = src.Temperature,
                    BloodPressure = src.BloodPressure,
                    HeartRate = src.HeartRate,
                    RespiratoryRate = src.RespiratoryRate,
                    Weight = src.Weight,
                    Height = src.Height
                }));

            CreateMap<CreateMedicalRecordDto, MedicalRecord>()
                .ForMember(dest => dest.Temperature, opt => opt.MapFrom(src => src.VitalSigns.Temperature))
                .ForMember(dest => dest.BloodPressure, opt => opt.MapFrom(src => src.VitalSigns.BloodPressure))
                .ForMember(dest => dest.HeartRate, opt => opt.MapFrom(src => src.VitalSigns.HeartRate))
                .ForMember(dest => dest.RespiratoryRate, opt => opt.MapFrom(src => src.VitalSigns.RespiratoryRate))
                .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.VitalSigns.Weight))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.VitalSigns.Height));
        }
    }
}