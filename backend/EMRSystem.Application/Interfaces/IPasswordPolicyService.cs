public interface IPasswordPolicyService
{
    Task<PasswordPolicy> GetAsync();
    Task<PasswordPolicy> UpdateAsync(PasswordPolicy policy, int adminUserId);
    Task<IList<string>> ValidateAsync(ApplicationUser user, string newPassword);
    Task SaveHistoryAsync(int userId, string passwordHash);
}