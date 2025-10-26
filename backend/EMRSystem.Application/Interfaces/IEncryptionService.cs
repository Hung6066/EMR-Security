// IEncryptionService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        string HashData(string data);
        bool VerifyHash(string data, string hash);
    }
}