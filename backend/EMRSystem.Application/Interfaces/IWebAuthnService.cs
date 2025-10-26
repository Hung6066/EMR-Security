// IWebAuthnService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IWebAuthnService
    {
        Task<CredentialCreateOptions> GetCredentialOptionsAsync(int userId);
        Task<bool> RegisterCredentialAsync(int userId, AuthenticatorAttestationRawResponse attestationResponse);
        Task<AssertionOptions> GetAssertionOptionsAsync(string email);
        Task<AuthResponseDto> VerifyAssertionAsync(string email, AuthenticatorAssertionRawResponse assertionResponse);
        Task<List<WebAuthnCredential>> GetUserCredentialsAsync(int userId);
        Task RevokeCredentialAsync(int credentialId);
    }
}