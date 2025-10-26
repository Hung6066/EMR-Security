// ApiKeyService.cs
using System.Security.Cryptography;
using System.Text;

namespace EMRSystem.Application.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApiKeyService> _logger;

        public ApiKeyService(ApplicationDbContext context, ILogger<ApiKeyService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiKeyDto> CreateApiKeyAsync(int userId, CreateApiKeyDto dto)
        {
            var apiKey = GenerateApiKey();
            var keyHash = HashApiKey(apiKey);
            var keyPrefix = apiKey.Substring(0, 10);

            var apiKeyEntity = new ApiKey
            {
                UserId = userId,
                Name = dto.Name,
                KeyHash = keyHash,
                KeyPrefix = keyPrefix,
                CreatedAt = DateTime.Now,
                ExpiresAt = dto.ExpiresAt,
                IsActive = true,
                IsRevoked = false,
                IpWhitelist = dto.IpWhitelist,
                AllowedScopes = dto.Scopes != null ? string.Join(",", dto.Scopes) : null,
                RateLimitPerMinute = dto.RateLimitPerMinute
            };

            _context.ApiKeys.Add(apiKeyEntity);
            await _context.SaveChangesAsync();

            return new ApiKeyDto
            {
                Id = apiKeyEntity.Id,
                Name = apiKeyEntity.Name,
                ApiKey = apiKey, // Only returned once
                KeyPrefix = keyPrefix,
                CreatedAt = apiKeyEntity.CreatedAt,
                ExpiresAt = apiKeyEntity.ExpiresAt
            };
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey, string ipAddress)
        {
            var keyPrefix = apiKey.Substring(0, 10);
            var keyHash = HashApiKey(apiKey);

            var key = await _context.ApiKeys
                .FirstOrDefaultAsync(k => k.KeyPrefix == keyPrefix && 
                                         k.KeyHash == keyHash &&
                                         k.IsActive && 
                                         !k.IsRevoked);

            if (key == null)
            {
                _logger.LogWarning($"Invalid API key attempt from IP: {ipAddress}");
                return false;
            }

            // Check expiration
            if (key.ExpiresAt.HasValue && key.ExpiresAt.Value < DateTime.Now)
            {
                _logger.LogWarning($"Expired API key used: {keyPrefix}");
                return false;
            }

            // Check IP whitelist
            if (!string.IsNullOrEmpty(key.IpWhitelist))
            {
                var allowedIps = key.IpWhitelist.Split(',').Select(ip => ip.Trim());
                if (!allowedIps.Contains(ipAddress))
                {
                    _logger.LogWarning($"API key {keyPrefix} used from unauthorized IP: {ipAddress}");
                    return false;
                }
            }

            // Update last used
            key.LastUsedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ApiKey> GetApiKeyInfoAsync(string keyPrefix)
        {
            return await _context.ApiKeys
                .Include(k => k.User)
                .FirstOrDefaultAsync(k => k.KeyPrefix == keyPrefix);
        }

        public async Task RevokeApiKeyAsync(int keyId)
        {
            var key = await _context.ApiKeys.FindAsync(keyId);
            if (key != null)
            {
                key.IsRevoked = true;
                key.RevokedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ApiKey>> GetUserApiKeysAsync(int userId)
        {
            return await _context.ApiKeys
                .Where(k => k.UserId == userId)
                .OrderByDescending(k => k.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateLastUsedAsync(string keyPrefix)
        {
            var key = await _context.ApiKeys.FirstOrDefaultAsync(k => k.KeyPrefix == keyPrefix);
            if (key != null)
            {
                key.LastUsedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        private string GenerateApiKey()
        {
            const string prefix = "emr_";
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return prefix + Convert.ToBase64String(randomBytes)
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "")
                .Substring(0, 40);
        }

        private string HashApiKey(string apiKey)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(apiKey);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}