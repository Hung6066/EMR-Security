// IPdfService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IPdfService
    {
        Task<byte[]> GenerateMedicalRecordPdfAsync(int recordId);
        Task<byte[]> GeneratePrescriptionPdfAsync(int prescriptionId);
    }
}