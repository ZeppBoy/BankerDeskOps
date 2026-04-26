using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankerDeskOps.Api.Controllers
{
    /// <summary>
    /// API controller for managing money transfers between accounts.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TransfersController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<TransfersController> _logger;

        public TransfersController(ITransactionService transactionService, ILogger<TransfersController> logger)
        {
            _transactionService = transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all transactions.
        /// </summary>
        /// <returns>Collection of all transactions.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TransactionDto>))]
        public async Task<ActionResult<IEnumerable<TransactionDto>>> GetAllTransactions()
        {
            _logger.LogInformation("Fetching all transactions");
            var transactions = await _transactionService.GetAllAsync();
            return Ok(transactions);
        }

        /// <summary>
        /// Retrieves a specific transaction by ID.
        /// </summary>
        /// <param name="id">The transaction ID.</param>
        /// <returns>The transaction if found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TransactionDto>> GetTransactionById(Guid id)
        {
            _logger.LogInformation("Fetching transaction with ID: {TransactionId}", id);
            var transaction = await _transactionService.GetByIdAsync(id);

            if (transaction is null)
            {
                _logger.LogWarning("Transaction with ID {TransactionId} not found", id);
                return NotFound();
            }

            return Ok(transaction);
        }

        /// <summary>
        /// Executes a money transfer between two accounts.
        /// </summary>
        /// <param name="request">The transfer request.</param>
        /// <returns>The completed transaction with entries.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TransactionDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TransactionDto>> ExecuteTransfer([FromBody] TransferRequest request)
        {
            if (request is null)
            {
                _logger.LogWarning("Transfer request is null");
                return BadRequest("Request cannot be null");
            }

            try
            {
                _logger.LogInformation(
                    "Processing transfer of {Amount} from account {FromAccountId} to account {ToAccountId}",
                    request.Amount,
                    request.FromAccountId,
                    request.ToAccountId);

                var transaction = await _transactionService.ExecuteTransferAsync(request);

                _logger.LogInformation(
                    "Transfer completed successfully. Transaction ID: {TransactionId}, Reference: {ReferenceId}",
                    transaction.Id,
                    transaction.ReferenceId);

                return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid transfer request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Transfer failed: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing transfer");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the transfer.");
            }
        }
    }
}