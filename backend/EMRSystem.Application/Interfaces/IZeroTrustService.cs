// IZeroTrustService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IZeroTrustService
    {
        Task<AccessDecisionResult> EvaluateAccessAsync(AccessRequest request);
        Task<TrustScoreResult> CalculateTrustScoreAsync(int userId, AccessContext context);
        Task<List<ZeroTrustPolicy>> GetApplicablePoliciesAsync(string resourceType, string resourcePath);
        Task<bool> VerifyDeviceComplianceAsync(string deviceFingerprint);
        Task<bool> VerifyNetworkComplianceAsync(string ipAddress);
        Task LogAccessDecisionAsync(AccessDecisionResult decision);
    }
    
    public class AccessRequest
    {
        public int UserId { get; set; }
        public string Resource { get; set; }
        public string Action { get; set; }
        public AccessContext Context { get; set; }
    }
    
    public class AccessContext
    {
        public string IpAddress { get; set; }
        public string DeviceFingerprint { get; set; }
        public string UserAgent { get; set; }
        public string Location { get; set; }
        public DateTime Timestamp { get; set; }
        public bool HasMFA { get; set; }
        public Dictionary<string, object> CustomAttributes { get; set; }
    }
    
    public class AccessDecisionResult
    {
        public bool IsAllowed { get; set; }
        public int TrustScore { get; set; }
        public List<string> DenialReasons { get; set; }
        public List<string> AppliedPolicies { get; set; }
        public Dictionary<string, object> RequiredActions { get; set; }
    }
    
    public class TrustScoreResult
    {
        public int OverallScore { get; set; }
        public int DeviceScore { get; set; }
        public int LocationScore { get; set; }
        public int BehaviorScore { get; set; }
        public int TimeScore { get; set; }
        public int NetworkScore { get; set; }
        public Dictionary<string, string> Details { get; set; }
    }
}