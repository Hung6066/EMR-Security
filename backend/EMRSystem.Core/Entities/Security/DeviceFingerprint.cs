// DeviceFingerprint.cs
namespace EMRSystem.Core.Entities
{
    public class DeviceFingerprint
    {
        public int Id { get; set; }
        
        [Required]
        public int? UserId { get; set; }
        
        [Required]
        [StringLength(64)]
        public string FingerprintHash { get; set; }
        
        public string UserAgent { get; set; }
        public string ScreenResolution { get; set; }
        public string Timezone { get; set; }
        public string Language { get; set; }
        public string Platform { get; set; }
        public bool CookiesEnabled { get; set; }
        public string Plugins { get; set; }
        public string CanvasFingerprint { get; set; }
        public string WebGLFingerprint { get; set; }
        public string AudioFingerprint { get; set; }
        public string Fonts { get; set; }
        
        public DateTime FirstSeenAt { get; set; }
        public DateTime LastSeenAt { get; set; }
        public int VisitCount { get; set; }
        
        public bool IsTrusted { get; set; }
        public DateTime? TrustedAt { get; set; }
        
        public int RiskScore { get; set; }
        
        public ApplicationUser User { get; set; }
    }
}