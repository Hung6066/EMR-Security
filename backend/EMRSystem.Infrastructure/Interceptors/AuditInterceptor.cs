// AuditInterceptor.cs
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace EMRSystem.Infrastructure.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditInterceptor(IAuditService auditService, IHttpContextAccessor httpContextAccessor)
        {
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            await AuditChangesAsync(eventData.Context);
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private async Task AuditChangesAsync(DbContext context)
        {
            if (context == null) return;

            var userId = GetCurrentUserId();
            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || 
                           e.State == EntityState.Modified || 
                           e.State == EntityState.Deleted)
                .Where(e => ShouldAudit(e.Entity.GetType()));

            foreach (var entry in entries)
            {
                var entityType = entry.Entity.GetType().Name;
                var entityId = GetEntityId(entry.Entity);
                var action = entry.State.ToString().ToUpper();

                object oldValues = null;
                object newValues = null;

                if (entry.State == EntityState.Modified)
                {
                    oldValues = GetOriginalValues(entry);
                    newValues = GetCurrentValues(entry);
                }
                else if (entry.State == EntityState.Added)
                {
                    newValues = GetCurrentValues(entry);
                }
                else if (entry.State == EntityState.Deleted)
                {
                    oldValues = GetCurrentValues(entry);
                }

                await _auditService.LogAsync(new AuditLogDto
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
        }

        private bool ShouldAudit(Type entityType)
        {
            // Audit specific entities
            var auditableTypes = new[]
            {
                typeof(Patient),
                typeof(MedicalRecord),
                typeof(Prescription),
                typeof(LabTest),
                typeof(MedicalDocument)
            };

            return auditableTypes.Contains(entityType);
        }

        private int? GetEntityId(object entity)
        {
            var idProperty = entity.GetType().GetProperty("Id");
            return idProperty?.GetValue(entity) as int?;
        }

        private object GetOriginalValues(EntityEntry entry)
        {
            var values = new Dictionary<string, object>();
            
            foreach (var property in entry.OriginalValues.Properties)
            {
                var originalValue = entry.OriginalValues[property];
                if (originalValue != null)
                {
                    values[property.Name] = originalValue;
                }
            }
            
            return values;
        }

        private object GetCurrentValues(EntityEntry entry)
        {
            var values = new Dictionary<string, object>();
            
            foreach (var property in entry.CurrentValues.Properties)
            {
                var currentValue = entry.CurrentValues[property];
                if (currentValue != null)
                {
                    values[property.Name] = currentValue;
                }
            }
            
            return values;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User
                .FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
    }
}