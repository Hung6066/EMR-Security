// IDlpService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IDlpService
    {
        Task<DlpScanResult> ScanContentAsync(string content, DlpScanType scanType);
        Task<bool> ValidateDataExportAsync(int userId, string dataType, int recordCount);
        Task LogDataExportAsync(int userId, string dataType, int recordCount, string destination);
        Task<bool> CheckSensitiveDataAccessAsync(int userId, string dataType);
    }
    
    public enum DlpScanType
    {
        Email,
        PhoneNumber,
        IdentityCard,
        CreditCard,
        MedicalInfo,
        All
    }
    
    public class DlpScanResult
    {
        public bool HasSensitiveData { get; set; }
        public List<SensitiveDataMatch> Matches { get; set; }
        public string RedactedContent { get; set; }
    }
    
    public class SensitiveDataMatch
    {
        public DlpScanType Type { get; set; }
        public string Value { get; set; }
        public int Position { get; set; }
    }
}