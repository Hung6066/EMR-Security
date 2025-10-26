// IAuditService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(AuditLogDto auditLog);
        Task LogAccessAsync(int userId, string entityType, int entityId, string action);
        Task LogDataChangeAsync(int userId, string entityType, int entityId, object oldValues, object newValues, string action);
        Task<List<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter);
        Task<List<AuditLog>> GetEntityAuditHistoryAsync(string entityType, int entityId);
    }
    
    public class AuditLogDto
    {
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public int? EntityId { get; set; }
        public object OldValues { get; set; }
        public object NewValues { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string AdditionalInfo { get; set; }
        public bool IsSuccess { get; set; }
        public string FailureReason { get; set; }
    }
    
    public class AuditLogFilter
    {
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}