// IAdvancedEncryptionService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IAdvancedEncryptionService
    {
        Task<EncryptedData> EncryptDataAsync(string plainText, string purpose);
        Task<string> DecryptDataAsync(EncryptedData encryptedData);
        Task<EncryptionKey> CreateKeyAsync(CreateKeyDto dto);
        Task RotateKeyAsync(int keyId);
        Task<string> GetFromVaultAsync(string keyName);
        Task StoreInVaultAsync(string keyName, string value, string description);
        Task<byte[]> HashWithSaltAsync(string data);
        Task<bool> VerifyHashAsync(string data, byte[] hash, byte[] salt);
    }
    
    public class EncryptedData
    {
        public byte[] CipherText { get; set; }
        public byte[] IV { get; set; }
        public int KeyId { get; set; }
        public string Algorithm { get; set; }
    }
    
    public class CreateKeyDto
    {
        public string KeyName { get; set; }
        public string KeyType { get; set; }
        public string Purpose { get; set; }
        public int KeySize { get; set; }
        public int ValidityDays { get; set; }
    }
}