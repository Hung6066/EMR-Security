// WebAuthnCredential.cs
namespace EMRSystem.Core.Entities
{
    public class WebAuthnCredential
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public byte[] CredentialId { get; set; }
        
        [Required]
        public byte[] PublicKey { get; set; }
        
        [Required]
        public byte[] UserHandle { get; set; }
        
        public uint SignatureCounter { get; set; }
        
        [Required]
        [StringLength(100)]
        public string CredentialType { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        
        [StringLength(200)]
        public string DeviceName { get; set; }
        
        public string AAGUID { get; set; }
        
        public bool IsActive { get; set; }
        
        public ApplicationUser User { get; set; }
    }
}