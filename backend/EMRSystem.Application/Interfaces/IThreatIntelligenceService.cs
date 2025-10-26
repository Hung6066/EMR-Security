// IThreatIntelligenceService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IThreatIntelligenceService
    {
        Task<ThreatAssessment> AssessIpAddressAsync(string ipAddress);
        Task<bool> IsIpBlacklistedAsync(string ipAddress);
        Task ReportSuspiciousActivityAsync(SuspiciousActivityReport report);
        Task<List<ThreatIndicator>> GetActiveThreatIndicatorsAsync();
        Task BlockIpAddressAsync(string ipAddress, string reason, TimeSpan? duration = null);
    }
    
    public class ThreatAssessment
    {
        public bool IsThreat { get; set; }
        public int ThreatScore { get; set; }
        public List<string> ThreatCategories { get; set; }
        public string CountryCode { get; set; }
        public bool IsVPN { get; set; }
        public bool IsProxy { get; set; }
        public bool IsTor { get; set; }
        public bool IsDataCenter { get; set; }
        public List<string> RecentAbuses { get; set; }
    }
    
    public class SuspiciousActivityReport
    {
        public string IpAddress { get; set; }
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
    
    public class ThreatIndicator
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public int Severity { get; set; }
        public DateTime DetectedAt { get; set; }
    }
}