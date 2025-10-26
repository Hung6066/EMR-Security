// TwoFactorDto.cs
namespace EMRSystem.Application.DTOs
{
    public class Enable2FADto
    {
        public string QrCodeUrl { get; set; }
        public string SecretKey { get; set; }
        public List<string> BackupCodes { get; set; }
    }
    
    public class Verify2FADto
    {
        [Required]
        public string Code { get; set; }
    }
    
    public class LoginWith2FADto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
        
        [Required]
        public string TwoFactorCode { get; set; }
        
        public string DeviceInfo { get; set; }
        public bool RememberDevice { get; set; }
    }
}