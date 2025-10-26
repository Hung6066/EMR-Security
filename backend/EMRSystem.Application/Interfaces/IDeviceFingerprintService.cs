// IDeviceFingerprintService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IDeviceFingerprintService
    {
        Task<DeviceFingerprint> CreateFingerprintAsync(DeviceFingerprintDto dto);
        Task<bool> ValidateFingerprintAsync(string fingerprint, int userId);
        Task<List<DeviceFingerprint>> GetTrustedDevicesAsync(int userId);
        Task TrustDeviceAsync(int userId, string fingerprint);
        Task RevokeTrustedDeviceAsync(int deviceId);
        Task<RiskScore> CalculateRiskScoreAsync(DeviceFingerprintDto dto, int? userId);
    }
    
    public class DeviceFingerprintDto
    {
        public string UserAgent { get; set; }
        public string ScreenResolution { get; set; }
        public string Timezone { get; set; }
        public string Language { get; set; }
        public string Platform { get; set; }
        public bool CookiesEnabled { get; set; }
        public List<string> Plugins { get; set; }
        public string CanvasFingerprint { get; set; }
        public string WebGLFingerprint { get; set; }
        public string AudioFingerprint { get; set; }
        public List<string> Fonts { get; set; }
    }
    
    public class RiskScore
    {
        public int Score { get; set; } // 0-100
        public string Level { get; set; } // Low, Medium, High
        public List<string> Reasons { get; set; }
        public bool RequiresAdditionalVerification { get; set; }
    }
}