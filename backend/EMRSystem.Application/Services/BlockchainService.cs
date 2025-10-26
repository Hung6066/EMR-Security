// BlockchainService.cs
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EMRSystem.Application.Services
{
    public class BlockchainService : IBlockchainService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BlockchainService> _logger;
        private const int DIFFICULTY = 4; // Number of leading zeros required

        public BlockchainService(
            ApplicationDbContext context,
            ILogger<BlockchainService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<BlockchainBlock> CreateBlockAsync(List<BlockchainTransaction> transactions)
        {
            var latestBlock = await GetLatestBlockAsync();
            var index = latestBlock?.Index + 1 ?? 0;
            var previousHash = latestBlock?.Hash ?? "0";

            var blockData = new
            {
                transactions = transactions.Select(t => new
                {
                    t.TransactionId,
                    t.TransactionType,
                    t.Payload,
                    t.UserId,
                    t.Timestamp
                })
            };

            var merkleRoot = CalculateMerkleRoot(transactions);

            var block = new BlockchainBlock
            {
                Index = index,
                Timestamp = DateTime.UtcNow,
                Data = JsonSerializer.Serialize(blockData),
                PreviousHash = previousHash,
                Difficulty = DIFFICULTY,
                MerkleRoot = merkleRoot,
                Nonce = 0,
                IsValid = true,
                CreatedAt = DateTime.Now
            };

            // Mine the block (Proof of Work)
            MineBlock(block);

            _context.BlockchainBlocks.Add(block);
            await _context.SaveChangesAsync();

            // Update transactions with block reference
            foreach (var transaction in transactions)
            {
                transaction.BlockId = block.Id;
                transaction.IsConfirmed = true;
            }
            await _context.SaveChangesAsync();

            _logger.LogInformation($"New block created: Index={block.Index}, Hash={block.Hash}");

            return block;
        }

        public async Task<BlockchainTransaction> AddTransactionAsync(string type, object data, int userId)
        {
            var transactionId = GenerateTransactionId();
            var payload = JsonSerializer.Serialize(data);
            var transactionHash = CalculateHash(transactionId + payload + userId + DateTime.UtcNow);

            var transaction = new BlockchainTransaction
            {
                TransactionId = transactionId,
                TransactionType = type,
                Payload = payload,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                TransactionHash = transactionHash,
                IsConfirmed = false
            };

            // Sign transaction
            transaction.Signature = SignTransaction(transaction);

            _context.BlockchainTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Auto-create block if pending transactions reach threshold
            var pendingCount = await _context.BlockchainTransactions
                .CountAsync(t => !t.IsConfirmed);

            if (pendingCount >= 10) // Configurable threshold
            {
                var pendingTransactions = await GetPendingTransactionsAsync();
                await CreateBlockAsync(pendingTransactions);
            }

            return transaction;
        }

        public async Task<bool> ValidateChainAsync()
        {
            var blocks = await _context.BlockchainBlocks
                .OrderBy(b => b.Index)
                .ToListAsync();

            if (blocks.Count == 0) return true;

            for (int i = 0; i < blocks.Count; i++)
            {
                var currentBlock = blocks[i];

                // Validate block hash
                var calculatedHash = CalculateBlockHash(currentBlock);
                if (currentBlock.Hash != calculatedHash)
                {
                    _logger.LogWarning($"Block {currentBlock.Index} has invalid hash");
                    currentBlock.IsValid = false;
                    await _context.SaveChangesAsync();
                    return false;
                }

                // Validate previous hash link
                if (i > 0)
                {
                    var previousBlock = blocks[i - 1];
                    if (currentBlock.PreviousHash != previousBlock.Hash)
                    {
                        _logger.LogWarning($"Block {currentBlock.Index} has invalid previous hash link");
                        currentBlock.IsValid = false;
                        await _context.SaveChangesAsync();
                        return false;
                    }
                }

                // Validate proof of work
                if (!IsValidProofOfWork(currentBlock.Hash, currentBlock.Difficulty))
                {
                    _logger.LogWarning($"Block {currentBlock.Index} has invalid proof of work");
                    currentBlock.IsValid = false;
                    await _context.SaveChangesAsync();
                    return false;
                }
            }

            return true;
        }

        public async Task<BlockchainBlock> GetLatestBlockAsync()
        {
            return await _context.BlockchainBlocks
                .OrderByDescending(b => b.Index)
                .FirstOrDefaultAsync();
        }

        public async Task<List<BlockchainBlock>> GetChainAsync(int skip = 0, int take = 100)
        {
            return await _context.BlockchainBlocks
                .OrderByDescending(b => b.Index)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<BlockchainTransaction> GetTransactionAsync(string transactionId)
        {
            return await _context.BlockchainTransactions
                .Include(t => t.Block)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }

        public async Task<List<BlockchainTransaction>> GetPendingTransactionsAsync()
        {
            return await _context.BlockchainTransactions
                .Where(t => !t.IsConfirmed)
                .OrderBy(t => t.Timestamp)
                .Take(100)
                .ToListAsync();
        }

        public async Task<BlockchainValidation> PerformIntegrityCheckAsync()
        {
            var blocks = await _context.BlockchainBlocks.ToListAsync();
            var validBlocks = 0;
            var invalidBlocks = 0;
            var details = new List<object>();

            foreach (var block in blocks)
            {
                var isValid = await ValidateBlockAsync(block);
                if (isValid)
                {
                    validBlocks++;
                }
                else
                {
                    invalidBlocks++;
                    details.Add(new
                    {
                        blockIndex = block.Index,
                        hash = block.Hash,
                        issue = "Hash validation failed"
                    });
                }
            }

            var validation = new BlockchainValidation
            {
                ValidationTime = DateTime.Now,
                IsValid = invalidBlocks == 0,
                TotalBlocks = blocks.Count,
                ValidBlocks = validBlocks,
                InvalidBlocks = invalidBlocks,
                ValidationDetails = JsonSerializer.Serialize(details),
                ValidatedByUserId = 1 // System
            };

            _context.BlockchainValidations.Add(validation);
            await _context.SaveChangesAsync();

            return validation;
        }

        public async Task<bool> VerifyTransactionAsync(string transactionId)
        {
            var transaction = await GetTransactionAsync(transactionId);
            if (transaction == null) return false;

            // Verify transaction hash
            var calculatedHash = CalculateHash(
                transaction.TransactionId + 
                transaction.Payload + 
                transaction.UserId + 
                transaction.Timestamp
            );

            if (transaction.TransactionHash != calculatedHash)
            {
                return false;
            }

            // Verify signature
            return VerifySignature(transaction);
        }

        private void MineBlock(BlockchainBlock block)
        {
            var target = new string('0', block.Difficulty);
            
            while (true)
            {
                block.Hash = CalculateBlockHash(block);
                
                if (block.Hash.StartsWith(target))
                {
                    break;
                }
                
                block.Nonce++;
            }
        }

        private string CalculateBlockHash(BlockchainBlock block)
        {
            var blockString = block.Index + 
                            block.Timestamp.ToString("O") + 
                            block.Data + 
                            block.PreviousHash + 
                            block.Nonce + 
                            block.MerkleRoot;
            
            return CalculateHash(blockString);
        }

        private string CalculateHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        private bool IsValidProofOfWork(string hash, int difficulty)
        {
            var target = new string('0', difficulty);
            return hash.StartsWith(target);
        }

        private string CalculateMerkleRoot(List<BlockchainTransaction> transactions)
        {
            if (transactions.Count == 0)
                return CalculateHash("");

            var hashes = transactions
                .Select(t => t.TransactionHash)
                .ToList();

            while (hashes.Count > 1)
            {
                var newHashes = new List<string>();
                
                for (int i = 0; i < hashes.Count; i += 2)
                {
                    var left = hashes[i];
                    var right = i + 1 < hashes.Count ? hashes[i + 1] : left;
                    var combined = CalculateHash(left + right);
                    newHashes.Add(combined);
                }
                
                hashes = newHashes;
            }

            return hashes[0];
        }

        private string GenerateTransactionId()
        {
            return Guid.NewGuid().ToString("N");
        }

        private string SignTransaction(BlockchainTransaction transaction)
        {
            // Simplified signing - in production, use proper digital signatures (RSA/ECDSA)
            var data = transaction.TransactionId + 
                      transaction.Payload + 
                      transaction.UserId;
            
            return CalculateHash(data + "secret_key");
        }

        private bool VerifySignature(BlockchainTransaction transaction)
        {
            var expectedSignature = SignTransaction(transaction);
            return transaction.Signature == expectedSignature;
        }

        private async Task<bool> ValidateBlockAsync(BlockchainBlock block)
        {
            var calculatedHash = CalculateBlockHash(block);
            return block.Hash == calculatedHash && 
                   IsValidProofOfWork(block.Hash, block.Difficulty);
        }
    }
}