// BlockchainController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMRSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BlockchainController : ControllerBase
    {
        private readonly IBlockchainService _blockchainService;

        public BlockchainController(IBlockchainService blockchainService)
        {
            _blockchainService = blockchainService;
        }

        [HttpGet("chain")]
        public async Task<ActionResult<List<BlockchainBlock>>> GetChain(
            [FromQuery] int skip = 0, 
            [FromQuery] int take = 100)
        {
            var chain = await _blockchainService.GetChainAsync(skip, take);
            return Ok(chain);
        }

        [HttpGet("latest")]
        public async Task<ActionResult<BlockchainBlock>> GetLatest()
        {
            var block = await _blockchainService.GetLatestBlockAsync();
            return Ok(block);
        }

        [HttpGet("transaction/{transactionId}")]
        public async Task<ActionResult<BlockchainTransaction>> GetTransaction(string transactionId)
        {
            var transaction = await _blockchainService.GetTransactionAsync(transactionId);
            if (transaction == null)
                return NotFound();
            
            return Ok(transaction);
        }

        [HttpPost("transaction")]
        public async Task<ActionResult<BlockchainTransaction>> AddTransaction(
            [FromBody] AddTransactionRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            
            var transaction = await _blockchainService.AddTransactionAsync(
                request.Type, 
                request.Data, 
                userId
            );
            
            return Ok(transaction);
        }

        [HttpPost("validate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<bool>> ValidateChain()
        {
            var isValid = await _blockchainService.ValidateChainAsync();
            return Ok(new { isValid });
        }

        [HttpPost("integrity-check")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlockchainValidation>> IntegrityCheck()
        {
            var validation = await _blockchainService.PerformIntegrityCheckAsync();
            return Ok(validation);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<List<BlockchainTransaction>>> GetPending()
        {
            var transactions = await _blockchainService.GetPendingTransactionsAsync();
            return Ok(transactions);
        }

        [HttpPost("verify/{transactionId}")]
        public async Task<ActionResult<bool>> VerifyTransaction(string transactionId)
        {
            var isValid = await _blockchainService.VerifyTransactionAsync(transactionId);
            return Ok(new { isValid });
        }

        [HttpPost("mine")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BlockchainBlock>> MineBlock()
        {
            var pendingTransactions = await _blockchainService.GetPendingTransactionsAsync();
            
            if (pendingTransactions.Count == 0)
                return BadRequest(new { message = "No pending transactions" });

            var block = await _blockchainService.CreateBlockAsync(pendingTransactions);
            return Ok(block);
        }
    }

    public class AddTransactionRequest
    {
        public string Type { get; set; }
        public object Data { get; set; }
    }
}