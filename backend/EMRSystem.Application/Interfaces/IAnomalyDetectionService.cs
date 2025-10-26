// IAnomalyDetectionService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IAnomalyDetectionService
    {
        Task<AnomalyScore> DetectUserBehaviorAnomalyAsync(int userId, UserActivity activity);
        Task<AnomalyScore> DetectDataAccessAnomalyAsync(DataAccessPattern pattern);
        Task<List<AnomalyAlert>> GetAnomaliesAsync(DateTime startDate, DateTime endDate);
        Task TrainModelAsync();
        Task<PredictionResult> PredictSecurityRiskAsync(SecurityContext context);
    }
    
    public class AnomalyScore
    {
        public double Score { get; set; } // 0-1, higher = more anomalous
        public bool IsAnomaly { get; set; }
        public List<string> Reasons { get; set; }
        public Dictionary<string, double> FeatureScores { get; set; }
    }
    
    public class UserActivity
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public string Resource { get; set; }
        public string IpAddress { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }
    
    public class DataAccessPattern
    {
        public int UserId { get; set; }
        public string DataType { get; set; }
        public int RecordCount { get; set; }
        public TimeSpan Duration { get; set; }
        public string AccessMethod { get; set; }
    }
    
    public class AnomalyAlert
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public double Score { get; set; }
        public string Description { get; set; }
        public DateTime DetectedAt { get; set; }
        public bool IsResolved { get; set; }
    }
    
    public class SecurityContext
    {
        public int UserId { get; set; }
        public string IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; }
        public Dictionary<string, object> Features { get; set; }
    }
    
    public class PredictionResult
    {
        public double RiskScore { get; set; }
        public string RiskLevel { get; set; }
        public List<string> RiskFactors { get; set; }
        public double Confidence { get; set; }
    }
}