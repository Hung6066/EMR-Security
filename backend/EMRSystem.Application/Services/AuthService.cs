// AuthService.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EMRSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly ApplicationDbContext _context;
    private readonly ITwoFactorService _twoFactorService;
    private readonly ISecurityService _securityService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtSettings> jwtSettings,
        ApplicationDbContext context,
        ITwoFactorService twoFactorService,
        ISecurityService securityService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
        _context = context;
        _twoFactorService = twoFactorService;
        _securityService = securityService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var ipAddress = GetClientIpAddress();
        var userAgent = GetUserAgent();

        // Check if account is locked
        if (await _securityService.IsAccountLockedAsync(dto.Email))
        {
            await _securityService.LogLoginAttemptAsync(
                dto.Email, ipAddress, userAgent, false, "Account locked");
            throw new Exception("Tài khoản đã bị khóa do quá nhiều lần đăng nhập thất bại. Vui lòng thử lại sau 15 phút.");
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
        {
            await _securityService.LogLoginAttemptAsync(
                dto.Email, ipAddress, userAgent, false, "Invalid credentials");
            throw new Exception("Email hoặc mật khẩu không đúng");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
        {
            await _securityService.LogLoginAttemptAsync(
                dto.Email, ipAddress, userAgent, false, "Invalid password");
            throw new Exception("Email hoặc mật khẩu không đúng");
        }

        // Check if 2FA is enabled
        var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        if (twoFactorEnabled)
        {
            // Return special response indicating 2FA is required
            return new AuthResponseDto
            {
                RequiresTwoFactor = true,
                UserId = user.Id,
                Email = user.Email
            };
        }

        // Log successful login
        await _securityService.LogLoginAttemptAsync(
            dto.Email, ipAddress, userAgent, true);

        user.LastLogin = DateTime.Now;
        await _userManager.UpdateAsync(user);

        var response = await GenerateAuthResponse(user);
        
        // Create session
        await _securityService.CreateSessionAsync(
            user.Id, response.AccessToken, ipAddress, userAgent, 
            GetDeviceInfo());

        return response;
    }

    public async Task<AuthResponseDto> LoginWith2FAAsync(LoginWith2FADto dto)
    {
        var ipAddress = GetClientIpAddress();
        var userAgent = GetUserAgent();

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            await _securityService.LogLoginAttemptAsync(
                dto.Email, ipAddress, userAgent, false, "Invalid credentials");
            throw new Exception("Đăng nhập thất bại");
        }

        // Verify password
        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
        {
            await _securityService.LogLoginAttemptAsync(
                dto.Email, ipAddress, userAgent, false, "Invalid password");
            throw new Exception("Đăng nhập thất bại");
        }

        // Verify 2FA code
        var is2FAValid = await _twoFactorService.VerifyTwoFactorCodeAsync(user.Id, dto.TwoFactorCode);
        
        if (!is2FAValid)
        {
            // Try backup code
            is2FAValid = await _twoFactorService.VerifyBackupCodeAsync(user.Id, dto.TwoFactorCode);
        }

        if (!is2FAValid)
        {
            await _securityService.LogLoginAttemptAsync(
                dto.Email, ipAddress, userAgent, false, "Invalid 2FA code");
            throw new Exception("Mã xác thực không đúng");
        }

        // Log successful login
        await _securityService.LogLoginAttemptAsync(
            dto.Email, ipAddress, userAgent, true);

        user.LastLogin = DateTime.Now;
        await _userManager.UpdateAsync(user);

        var response = await GenerateAuthResponse(user);
        
        // Create session
        await _securityService.CreateSessionAsync(
            user.Id, response.AccessToken, ipAddress, userAgent, 
            dto.DeviceInfo ?? GetDeviceInfo());

        return response;
    }

    private string GetClientIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString() ?? "Unknown";
    }

    private string GetDeviceInfo()
    {
        var userAgent = GetUserAgent();
        // Parse user agent to get device info
        if (userAgent.Contains("Mobile")) return "Mobile Device";
        if (userAgent.Contains("Tablet")) return "Tablet";
        return "Desktop";
    }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new Exception("Email already registered");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, dto.Role);

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !user.IsActive)
                throw new Exception("Invalid credentials");

            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded)
                throw new Exception("Invalid credentials");

            user.LastLogin = DateTime.Now;
            await _userManager.UpdateAsync(user);

            return await GenerateAuthResponse(user);
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

            if (token == null || token.ExpiryDate < DateTime.Now)
                throw new Exception("Invalid or expired refresh token");

            return await GenerateAuthResponse(token.User);
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (token == null)
                return false;

            token.IsRevoked = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new Exception("User not found");

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

            return true;
        }

        private async Task<AuthResponseDto> GenerateAuthResponse(ApplicationUser user)
        {
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            // Save refresh token
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.Now.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedAt = DateTime.Now,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Doctor") || roles.Contains("Admin"))
            {
                claims.Add(new Claim("permission", "ViewFullPII"));
            }

            return new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.Now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                Roles = roles.ToList()
            };
        }

        private string GenerateAccessToken(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var roles = _userManager.GetRolesAsync(user).Result;
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

        public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false; // Don't reveal that user doesn't exist

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _emailService.SendPasswordResetEmailAsync(email, token);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new Exception("Invalid request");

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Send security alert
        await _securityService.SendSecurityAlertAsync(user.Id, "PasswordChanged",
            "Mật khẩu của bạn đã được thay đổi thành công.");

            await _passwordPolicyService.SaveHistoryAsync(user.Id, (await _userManager.GetPasswordHashAsync(user)));

        return true;
    }

    public async Task<bool> VerifyEmailAsync(string email, string token)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new Exception("Invalid request");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }
}