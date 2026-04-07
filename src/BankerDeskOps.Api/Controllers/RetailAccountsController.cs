using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankerDeskOps.Api.Controllers
{
    /// <summary>
    /// API controller for managing retail accounts.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RetailAccountsController : ControllerBase
    {
        private readonly IRetailAccountService _accountService;
        private readonly ILogger<RetailAccountsController> _logger;

        public RetailAccountsController(IRetailAccountService accountService, ILogger<RetailAccountsController> logger)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all retail accounts.
        /// </summary>
        /// <returns>Collection of all accounts.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RetailAccountDto>))]
        public async Task<ActionResult<IEnumerable<RetailAccountDto>>> GetAllAccounts()
        {
            _logger.LogInformation("Fetching all retail accounts");
            var accounts = await _accountService.GetAllAsync();
            return Ok(accounts);
        }

        /// <summary>
        /// Retrieves a specific account by ID.
        /// </summary>
        /// <param name="id">The account ID.</param>
        /// <returns>The account if found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RetailAccountDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RetailAccountDto>> GetAccountById(Guid id)
        {
            _logger.LogInformation("Fetching account with ID: {AccountId}", id);
            var account = await _accountService.GetByIdAsync(id);

            if (account is null)
            {
                _logger.LogWarning("Account with ID {AccountId} not found", id);
                return NotFound();
            }

            return Ok(account);
        }

        /// <summary>
        /// Opens a new retail account.
        /// </summary>
        /// <param name="request">The create account request.</param>
        /// <returns>The created account.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RetailAccountDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RetailAccountDto>> OpenAccount([FromBody] CreateRetailAccountRequest request)
        {
            if (request is null)
            {
                _logger.LogWarning("Create account request is null");
                return BadRequest("Request cannot be null");
            }

            try
            {
                _logger.LogInformation("Opening new account for customer: {CustomerName}", request.CustomerName);
                var createdAccount = await _accountService.OpenAsync(request);

                return CreatedAtAction(nameof(GetAccountById), new { id = createdAccount.Id }, createdAccount);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid account creation request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating account: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        /// <summary>
        /// Deposits funds into an account.
        /// </summary>
        /// <param name="id">The account ID.</param>
        /// <param name="request">The deposit request.</param>
        /// <returns>The updated account.</returns>
        [HttpPost("{id}/deposit")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RetailAccountDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RetailAccountDto>> Deposit(Guid id, [FromBody] DepositRequest request)
        {
            if (request is null)
            {
                _logger.LogWarning("Deposit request is null for account {AccountId}", id);
                return BadRequest("Request cannot be null");
            }

            try
            {
                _logger.LogInformation("Processing deposit of {Amount} to account {AccountId}", request.Amount, id);
                var updatedAccount = await _accountService.DepositAsync(id, request);
                return Ok(updatedAccount);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Deposit failed: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing deposit: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        /// <summary>
        /// Withdraws funds from an account.
        /// </summary>
        /// <param name="id">The account ID.</param>
        /// <param name="request">The withdraw request.</param>
        /// <returns>The updated account.</returns>
        [HttpPost("{id}/withdraw")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RetailAccountDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RetailAccountDto>> Withdraw(Guid id, [FromBody] WithdrawRequest request)
        {
            if (request is null)
            {
                _logger.LogWarning("Withdraw request is null for account {AccountId}", id);
                return BadRequest("Request cannot be null");
            }

            try
            {
                _logger.LogInformation("Processing withdrawal of {Amount} from account {AccountId}", request.Amount, id);
                var updatedAccount = await _accountService.WithdrawAsync(id, request);
                return Ok(updatedAccount);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Withdrawal failed: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error processing withdrawal: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        /// <summary>
        /// Closes an account.
        /// </summary>
        /// <param name="id">The account ID to close.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CloseAccount(Guid id)
        {
            _logger.LogInformation("Closing account with ID: {AccountId}", id);
            var closed = await _accountService.CloseAsync(id);

            if (!closed)
            {
                _logger.LogWarning("Account with ID {AccountId} not found for closure", id);
                return NotFound();
            }

            return NoContent();
        }
    }
}
