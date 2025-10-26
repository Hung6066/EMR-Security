// BlockchainAuditTrail.cs
namespace EMRSystem.Core.Entities
{
    public class BlockchainBlock
    {
        public long Id { get; set; }
        
        [Required]
        public int Index { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        [Required]
        public string Data { get; set; } // JSON of audit data
        
        [Required]
        [StringLength(64)]
        public string Hash { get; set; }
        
        [Required]
        [StringLength(64)]
        public string PreviousHash { get; set; }
        
        public int Nonce { get; set; }
        
        public int Difficulty { get; set; }
        
        public string MerkleRoot { get; set; }
        
        public bool IsValid { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
    
    public class BlockchainTransaction
    {
        public long Id { get; set; }
        
        [Required]
        public string TransactionId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string TransactionType { get; set; }
        
        [Required]
        public string Payload { get; set; } // JSON
        
        public int UserId { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public long? BlockId { get; set; }
        
        [StringLength(64)]
        public string TransactionHash { get; set; }
        
        public string Signature { get; set; }
        
        public bool IsConfirmed { get; set; }
        
        public BlockchainBlock Block { get; set; }
        public ApplicationUser User { get; set; }
    }
    
    public class BlockchainValidation
    {
        public long Id { get; set; }
        
        public DateTime ValidationTime { get; set; }
        
        public bool IsValid { get; set; }
        
        public int TotalBlocks { get; set; }
        public int ValidBlocks { get; set; }
        public int InvalidBlocks { get; set; }
        
        public string ValidationDetails { get; set; } // JSON
        
        public int ValidatedByUserId { get; set; }
        
        public ApplicationUser ValidatedBy { get; set; }
    }
}