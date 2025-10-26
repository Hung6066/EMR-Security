// EncryptionKey.cs
namespace EMRSystem.Core.Entities
{
    public class EncryptionKey
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string KeyName { get; set; }
        
        [Required]
        [StringLength(50)]
        public string KeyType { get; set; } // AES, RSA, HMAC
        
        [Required]
        public byte[] EncryptedKey { get; set; }
        
        public byte[] IV { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Purpose { get; set; } // DataEncryption, Signing, TokenEncryption
        
        public int KeySize { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        
        public bool IsActive { get; set; }
        public bool IsRotated { get; set; }
        
        public int? RotatedToKeyId { get; set; }
        public DateTime? RotatedAt { get; set; }
        
        public int CreatedByUserId { get; set; }
        
        public ApplicationUser CreatedBy { get; set; }
    }
    
    public class DataEncryptionAudit
    {
        public long Id { get; set; }
        
        [Required]
        public int EncryptionKeyId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string DataType { get; set; }
        
        public int? RecordId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Operation { get; set; } // Encrypt, Decrypt
        
        public int PerformedByUserId { get; set; }
        
        public DateTime PerformedAt { get; set; }
        
        public string AdditionalInfo { get; set; }
        
        public EncryptionKey EncryptionKey { get; set; }
        public ApplicationUser PerformedBy { get; set; }
    }
    
    public class SecureVault
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string KeyName { get; set; }
        
        [Required]
        public byte[] EncryptedValue { get; set; }
        
        public byte[] IV { get; set; }
        
        [Required]
        public int EncryptionKeyId { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        
        public string AccessPolicy { get; set; } // JSON
        
        public EncryptionKey EncryptionKey { get; set; }
    }
}