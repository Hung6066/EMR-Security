namespace EMRSystem.Core.Entities.Security
{
    public class PasswordPolicy
    {
        public int Id { get; set; }
        public int MinLength { get; set; } = 8;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireNonAlphanumeric { get; set; } = false;
        public int RequiredUniqueChars { get; set; } = 1;

        public int ExpireDays { get; set; } = 90;
        public int PasswordHistory { get; set; } = 5; // Không cho lặp lại N mật khẩu gần nhất
        public bool CheckCommonPasswords { get; set; } = true;
        public bool CheckPwnedPasswords { get; set; } = true; // HIBP
        public int MaxFailedAccessAttempts { get; set; } = 5;
        public int LockoutMinutes { get; set; } = 15;

        public DateTime UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class PasswordHistoryItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}