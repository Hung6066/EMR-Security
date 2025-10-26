// TwoFactorService.cs
using Google.Authenticator;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;

namespace EMRSystem.Application.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TwoFactorAuthenticator _tfa;

        public TwoFactorService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _tfa = new TwoFactorAuthenticator();
        }

        public async Task<Enable2FADto> EnableTwoFactorAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new Exception("User not found");

            // Generate secret key
            var secretKey = GenerateSecretKey();

            // Create or update 2FA record
            var twoFactor = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (twoFactor == null)
            {
                twoFactor = new TwoFactorAuth
                {
                    UserId = userId,
                    SecretKey = secretKey,
                    IsEnabled = false,
                    CreatedAt = DateTime.Now
                };
                _context.TwoFactorAuths.Add(twoFactor);
            }
            else
            {
                twoFactor.SecretKey = secretKey;
            }

            await _context.SaveChangesAsync();

            // Generate QR Code
            var setupInfo = _tfa.GenerateSetupCode(
                "EMR System",
                user.Email,
                secretKey,
                false,
                3
            );

            // Generate backup codes
            var backupCodes = await GenerateBackupCodesAsync(userId);

            return new Enable2FADto
            {
                QrCodeUrl = setupInfo.QrCodeSetupImageUrl,
                SecretKey = secretKey,
                BackupCodes = backupCodes
            };
        }

        public async Task<bool> VerifyAndEnableTwoFactorAsync(int userId, string code)
        {
            var twoFactor = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (twoFactor == null)
                return false;

            var isValid = _tfa.ValidateTwoFactorPIN(twoFactor.SecretKey, code);

            if (isValid)
            {
                twoFactor.IsEnabled = true;
                twoFactor.EnabledAt = DateTime.Now;
                
                var user = await _userManager.FindByIdAsync(userId.ToString());
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                
                await _context.SaveChangesAsync();
            }

            return isValid;
        }

        public async Task<bool> DisableTwoFactorAsync(int userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return false;

            // Verify password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                return false;

            var twoFactor = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (twoFactor != null)
            {
                twoFactor.IsEnabled = false;
                await _userManager.SetTwoFactorEnabledAsync(user, false);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> VerifyTwoFactorCodeAsync(int userId, string code)
        {
            var twoFactor = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(t => t.UserId == userId && t.IsEnabled);

            if (twoFactor == null)
                return false;

            return _tfa.ValidateTwoFactorPIN(twoFactor.SecretKey, code);
        }

        public async Task<bool> VerifyBackupCodeAsync(int userId, string code)
        {
            var backupCode = await _context.TwoFactorBackupCodes
                .FirstOrDefaultAsync(b => b.UserId == userId && 
                                         b.Code == code && 
                                         !b.IsUsed);

            if (backupCode == null)
                return false;

            backupCode.IsUsed = true;
            backupCode.UsedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<string>> RegenerateBackupCodesAsync(int userId)
        {
            // Remove old backup codes
            var oldCodes = await _context.TwoFactorBackupCodes
                .Where(b => b.UserId == userId)
                .ToListAsync();

            _context.TwoFactorBackupCodes.RemoveRange(oldCodes);

            // Generate new codes
            return await GenerateBackupCodesAsync(userId);
        }

        private async Task<List<string>> GenerateBackupCodesAsync(int userId)
        {
            var codes = new List<string>();

            for (int i = 0; i < 10; i++)
            {
                var code = GenerateBackupCode();
                codes.Add(code);

                _context.TwoFactorBackupCodes.Add(new TwoFactorBackupCode
                {
                    UserId = userId,
                    Code = code,
                    IsUsed = false,
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return codes;
        }

        private string GenerateSecretKey()
        {
            var bytes = new byte[20];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        private string GenerateBackupCode()
        {
            var bytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return BitConverter.ToUInt32(bytes, 0).ToString("D8");
        }
    }
}