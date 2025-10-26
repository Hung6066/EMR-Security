// IBlockchainService.cs
namespace EMRSystem.Application.Interfaces
{
    public interface IBlockchainService
    {
        Task<BlockchainBlock> CreateBlockAsync(List<BlockchainTransaction> transactions);
        Task<BlockchainTransaction> AddTransactionAsync(string type, object data, int userId);
        Task<bool> ValidateChainAsync();
        Task<BlockchainBlock> GetLatestBlockAsync();
        Task<List<BlockchainBlock>> GetChainAsync(int skip = 0, int take = 100);
        Task<BlockchainTransaction> GetTransactionAsync(string transactionId);
        Task<List<BlockchainTransaction>> GetPendingTransactionsAsync();
        Task<BlockchainValidation> PerformIntegrityCheckAsync();
        Task<bool> VerifyTransactionAsync(string transactionId);
    }
}