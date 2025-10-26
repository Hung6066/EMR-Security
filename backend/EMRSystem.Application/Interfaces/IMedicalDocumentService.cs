// IMedicalDocumentService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IMedicalDocumentService
    {
        Task<MedicalDocumentDto> UploadDocumentAsync(int medicalRecordId, IFormFile file, string description, int userId);
        Task<IEnumerable<MedicalDocumentDto>> GetDocumentsByRecordIdAsync(int medicalRecordId);
        Task<byte[]> DownloadDocumentAsync(int documentId);
        Task DeleteDocumentAsync(int documentId);
    }
}