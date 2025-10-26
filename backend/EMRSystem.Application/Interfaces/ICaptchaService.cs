// ICaptchaService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface ICaptchaService
    {
        Task<bool> VerifyTokenAsync(string token, string remoteIp);
        Task<CaptchaValidationResult> ValidateV3Async(string token, string action);
    }
    
    public class CaptchaValidationResult
    {
        public bool Success { get; set; }
        public double Score { get; set; }
        public string Action { get; set; }
        public List<string> ErrorCodes { get; set; }
    }
}