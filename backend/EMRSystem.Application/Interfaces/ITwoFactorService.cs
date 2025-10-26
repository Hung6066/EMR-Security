// ITwoFactorService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface ITwoFactorService
    {
        Task<Enable2FADto> EnableTwoFactorAsync(int userId);
        Task<bool> VerifyAndEnableTwoFactorAsync(int userId, string code);
        Task<bool> DisableTwoFactorAsync(int userId, string password);
        Task<bool> VerifyTwoFactorCodeAsync(int userId, string code);
        Task<bool> VerifyBackupCodeAsync(int userId, string code);
        Task<List<string>> RegenerateBackupCodesAsync(int userId);
    }
}