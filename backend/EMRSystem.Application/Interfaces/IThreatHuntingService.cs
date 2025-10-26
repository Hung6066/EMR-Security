// IThreatHuntingService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IThreatHuntingService
    {
        Task<ThreatHuntingResult> ExecuteQueryAsync(int queryId);
        Task<ThreatHuntingQuery> CreateQueryAsync(CreateThreatQueryDto dto);
        Task<List<ThreatHuntingQuery>> GetQueriesAsync();
        Task<List<ThreatHuntingResult>> GetResultsAsync(int queryId);
        Task<List<ThreatIndicator>> GetIndicatorsAsync();
        Task<ThreatIndicator> AddIndicatorAsync(AddThreatIndicatorDto dto);
        Task<ThreatHuntingSummary> GetSummaryAsync(DateTime startDate, DateTime endDate);
        Task<List<SuspiciousActivity>> HuntSuspiciousActivitiesAsync(HuntingCriteria criteria);
    }
    
    public class CreateThreatQueryDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public QueryDefinition Definition { get; set; }
        public string Severity { get; set; }
    }
    
    public class QueryDefinition
    {
        public string DataSource { get; set; }
        public List<QueryCondition> Conditions { get; set; }
        public string TimeRange { get; set; }
    }
    
    public class QueryCondition
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
    }
    
    public class AddThreatIndicatorDto
    {
        public string IndicatorType { get; set; }
        public string Value { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public string Source { get; set; }
    }
    
    public class ThreatHuntingSummary
    {
        public int TotalQueries { get; set; }
        public int TotalExecutions { get; set; }
        public int ThreatsDetected { get; set; }
        public Dictionary<string, int> ThreatsBySeverity { get; set; }
    }
    
    public class HuntingCriteria
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> ActivityTypes { get; set; }
        public int? UserId { get; set; }
        public string IpAddress { get; set; }
    }
    
    public class SuspiciousActivity
    {
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime Timestamp { get; set; }
        public double ThreatScore { get; set; }
        public List<string> Indicators { get; set; }
    }
}