// ISecurityIncidentService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface ISecurityIncidentService
    {
        Task<SecurityIncident> CreateIncidentAsync(CreateIncidentDto dto);
        Task<SecurityIncident> GetIncidentByIdAsync(long id);
        Task<List<SecurityIncident>> GetActiveIncidentsAsync();
        Task UpdateIncidentStatusAsync(long id, string status, string notes);
        Task AssignIncidentAsync(long id, int userId);
        Task AddCommentAsync(long incidentId, int userId, string comment);
        Task AddActionAsync(long incidentId, IncidentActionDto action);
        Task<IncidentPlaybook> GetPlaybookAsync(string category, string severity);
        Task<bool> ExecuteAutomatedResponseAsync(SecurityIncident incident);
        Task<IncidentMetrics> GetIncidentMetricsAsync(DateTime startDate, DateTime endDate);
    }
    
    public class CreateIncidentDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public string Category { get; set; }
        public int? AffectedUserId { get; set; }
        public string AffectedResource { get; set; }
        public string IpAddress { get; set; }
        public Dictionary<string, object> Evidence { get; set; }
    }
    
    public class IncidentActionDto
    {
        public string ActionType { get; set; }
        public string Description { get; set; }
        public int PerformedByUserId { get; set; }
        public string Result { get; set; }
    }
    
    public class IncidentMetrics
    {
        public int TotalIncidents { get; set; }
        public int CriticalIncidents { get; set; }
        public int ResolvedIncidents { get; set; }
        public double AverageResolutionTime { get; set; }
        public Dictionary<string, int> IncidentsByCategory { get; set; }
        public Dictionary<string, int> IncidentsBySeverity { get; set; }
    }
}