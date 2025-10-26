public class PasswordPolicyService : IPasswordPolicyService
{
    private readonly ApplicationDbContext _ctx;
    private readonly UserManager<ApplicationUser> _um;
    private static readonly HashSet<string> Common = new(StringComparer.OrdinalIgnoreCase)
    {
        "password","123456","qwerty","111111","abc123","letmein"
    };

    public PasswordPolicyService(ApplicationDbContext ctx, UserManager<ApplicationUser> um)
    {
        _ctx = ctx; _um = um;
    }

    public async Task<PasswordPolicy> GetAsync()
    {
        var policy = await _ctx.PasswordPolicies.FirstOrDefaultAsync();
        if (policy == null)
        {
            policy = new PasswordPolicy { UpdatedAt = DateTime.UtcNow };
            _ctx.PasswordPolicies.Add(policy);
            await _ctx.SaveChangesAsync();
        }
        return policy;
    }

    public async Task<PasswordPolicy> UpdateAsync(PasswordPolicy policy, int adminUserId)
    {
        var current = await GetAsync();
        _ctx.Entry(current).CurrentValues.SetValues(policy);
        current.UpdatedAt = DateTime.UtcNow;
        current.UpdatedBy = adminUserId;
        await _ctx.SaveChangesAsync();
        return current;
    }

    public async Task<IList<string>> ValidateAsync(ApplicationUser user, string newPassword)
    {
        var errors = new List<string>();
        var p = await GetAsync();

        if (newPassword.Length < p.MinLength) errors.Add($"Mật khẩu tối thiểu {p.MinLength} ký tự");
        if (p.RequireLowercase && !newPassword.Any(char.IsLower)) errors.Add("Cần chữ thường");
        if (p.RequireUppercase && !newPassword.Any(char.IsUpper)) errors.Add("Cần chữ hoa");
        if (p.RequireDigit && !newPassword.Any(char.IsDigit)) errors.Add("Cần chữ số");
        if (p.RequireNonAlphanumeric && newPassword.All(ch => char.IsLetterOrDigit(ch)))
            errors.Add("Cần ký tự đặc biệt");
        if (p.CheckCommonPasswords && Common.Contains(newPassword)) errors.Add("Mật khẩu quá phổ biến");

        // Lịch sử N mật khẩu gần nhất
        if (p.PasswordHistory > 0)
        {
            var history = await _ctx.PasswordHistory
                .Where(h => h.UserId == user.Id)
                .OrderByDescending(h => h.CreatedAt).Take(p.PasswordHistory).ToListAsync();

            foreach (var h in history)
            {
                if (_um.PasswordHasher.VerifyHashedPassword(user, h.PasswordHash, newPassword) != PasswordVerificationResult.Failed)
                {
                    errors.Add($"Không được dùng trùng {p.PasswordHistory} mật khẩu gần nhất");
                    break;
                }
            }
        }

        // Kiểm tra HIBP (k-anonymity)
        if (p.CheckPwnedPasswords)
        {
            var sha1 = System.Security.Cryptography.SHA1.HashData(System.Text.Encoding.UTF8.GetBytes(newPassword));
            var hash = string.Concat(sha1.Select(b => b.ToString("x2"))).ToUpperInvariant();
            var prefix = hash.Substring(0, 5);
            var suffix = hash.Substring(5);

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "EMRSystem");
            var resp = await http.GetStringAsync($"https://api.pwnedpasswords.com/range/{prefix}");
            // Resp: lines "SUFFIX:count"
            if (resp.Split('\n').Any(line => line.StartsWith(suffix, StringComparison.OrdinalIgnoreCase)))
                errors.Add("Mật khẩu đã bị lộ (HIBP). Vui lòng dùng mật khẩu khác.");
        }

        return errors;
    }

    public async Task SaveHistoryAsync(int userId, string passwordHash)
    {
        _ctx.PasswordHistory.Add(new PasswordHistoryItem
        {
            UserId = userId,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        });
        await _ctx.SaveChangesAsync();
    }
}