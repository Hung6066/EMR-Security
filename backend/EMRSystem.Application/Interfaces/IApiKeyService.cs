.// IApiKeyService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IApiKeyService
    {
        Task<ApiKeyDto> CreateApiKeyAsync(int userId, CreateApiKeyDto dto);
        Task<bool> ValidateApiKeyAsync(string apiKey, string ipAddress);
        Task<ApiKey> GetApiKeyInfoAsync(string keyPrefix);
        Task RevokeApiKeyAsync(int keyId);
        Task<List<ApiKey>> GetUserApiKeysAsync(int userId);
        Task UpdateLastUsedAsync(string keyPrefix);
    }
    
    public class CreateApiKeyDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        public DateTime? ExpiresAt { get; set; }
        public string IpWhitelist { get; set; }
        public List<string> Scopes { get; set; }
        public int? RateLimitPerMinute { get; set; }
    }
    
    public class ApiKeyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ApiKey { get; set; } // Only returned once on creation
        public string KeyPrefix { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}