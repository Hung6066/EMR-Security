// EncryptionInterceptor.cs
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace EMRSystem.Infrastructure.Interceptors
{
    public class EncryptionInterceptor : SaveChangesInterceptor
    {
        private readonly IEncryptionService _encryptionService;

        public EncryptionInterceptor(IEncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            EncryptSensitiveData(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            EncryptSensitiveData(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void EncryptSensitiveData(DbContext context)
        {
            if (context == null) return;

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var properties = entry.Entity.GetType().GetProperties()
                    .Where(p => p.GetCustomAttributes(typeof(SensitiveDataAttribute), false).Any());

                foreach (var property in properties)
                {
                    var value = property.GetValue(entry.Entity)?.ToString();
                    if (!string.IsNullOrEmpty(value) && !IsEncrypted(value))
                    {
                        var encrypted = _encryptionService.Encrypt(value);
                        property.SetValue(entry.Entity, encrypted);
                    }
                }
            }
        }

        private bool IsEncrypted(string value)
        {
            // Simple check - encrypted values are Base64
            try
            {
                Convert.FromBase64String(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}