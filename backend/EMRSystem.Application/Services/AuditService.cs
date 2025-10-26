// AuditService.cs
using System.Text.Json;

namespace EMRSystem.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(AuditLogDto auditLog)
        {
            var log = new AuditLog
            {
                UserId = auditLog.UserId,
                Action = auditLog.Action,
                EntityType = auditLog.EntityType,
                EntityId = auditLog.EntityId,
                OldValues = auditLog.OldValues != null ? JsonSerializer.Serialize(auditLog.OldValues) : null,
                NewValues = auditLog.NewValues != null ? JsonSerializer.Serialize(auditLog.NewValues) : null,
                Timestamp = DateTime.Now,
                IpAddress = auditLog.IpAddress ?? GetClientIpAddress(),
                UserAgent = auditLog.UserAgent ?? GetUserAgent(),
                AdditionalInfo = auditLog.AdditionalInfo,
                IsSuccess = auditLog.IsSuccess,
                FailureReason = auditLog.FailureReason
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task LogAccessAsync(int userId, string entityType, int entityId, string action)
        {
            await LogAsync(new AuditLogDto
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                IsSuccess = true
            });
        }

        public async Task LogDataChangeAsync(
            int userId, 
            string entityType, 
            int entityId, 
            object oldValues, 
            object newValues, 
            string action)
        {
            await LogAsync(new AuditLogDto
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                OldValues = oldValues,
                NewValues = newValues,
                IsSuccess = true
            });
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(AuditLogFilter filter)
        {
            var query = _context.AuditLogs.Include(a => a.User).AsQueryable();

            if (filter.UserId.HasValue)
                query = query.Where(a => a.UserId == filter.UserId);

            if (!string.IsNullOrEmpty(filter.Action))
                query = query.Where(a => a.Action == filter.Action);

            if (!string.IsNullOrEmpty(filter.EntityType))
                query = query.Where(a => a.EntityType == filter.EntityType);

            if (filter.StartDate.HasValue)
                query = query.Where(a => a.Timestamp >= filter.StartDate);

            if (filter.EndDate.HasValue)
                query = query.Where(a => a.Timestamp <= filter.EndDate);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetEntityAuditHistoryAsync(string entityType, int entityId)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        private string GetClientIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
        }
    }
}