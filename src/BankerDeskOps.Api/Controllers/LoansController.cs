using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankerDeskOps.Api.Controllers
{
    /// <summary>
    /// API controller for managing loans.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;
        private readonly ILogger<LoansController> _logger;

        public LoansController(ILoanService loanService, ILogger<LoansController> logger)
        {
            _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all loans.
        /// </summary>
        /// <returns>Collection of all loans.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<LoanDto>))]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetAllLoans()
        {
            _logger.LogInformation("Fetching all loans");
            var loans = await _loanService.GetAllAsync();
            return Ok(loans);
        }

        /// <summary>
        /// Retrieves a specific loan by ID.
        /// </summary>
        /// <param name="id">The loan ID.</param>
        /// <returns>The loan if found.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoanDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanDto>> GetLoanById(Guid id)
        {
            _logger.LogInformation("Fetching loan with ID: {LoanId}", id);
            var loan = await _loanService.GetByIdAsync(id);

            if (loan is null)
            {
                _logger.LogWarning("Loan with ID {LoanId} not found", id);
                return NotFound();
            }

            return Ok(loan);
        }

        /// <summary>
        /// Creates a new loan.
        /// </summary>
        /// <param name="request">The create loan request.</param>
        /// <returns>The created loan.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(LoanDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoanDto>> CreateLoan([FromBody] CreateLoanRequest request)
        {
            if (request is null)
            {
                _logger.LogWarning("Create loan request is null");
                return BadRequest("Request cannot be null");
            }

            try
            {
                _logger.LogInformation("Creating new loan for customer: {CustomerName}", request.CustomerName);
                var createdLoan = await _loanService.CreateAsync(request);

                return CreatedAtAction(nameof(GetLoanById), new { id = createdLoan.Id }, createdLoan);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid loan creation request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating loan: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        /// <summary>
        /// Approves a loan.
        /// </summary>
        /// <param name="id">The loan ID to approve.</param>
        /// <returns>The updated loan.</returns>
        [HttpPut("{id}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoanDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoanDto>> ApproveLoan(Guid id)
        {
            try
            {
                _logger.LogInformation("Approving loan with ID: {LoanId}", id);
                var updatedLoan = await _loanService.ApproveAsync(id);
                return Ok(updatedLoan);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Loan approval failed: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error approving loan: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        /// <summary>
        /// Rejects a loan.
        /// </summary>
        /// <param name="id">The loan ID to reject.</param>
        /// <returns>The updated loan.</returns>
        [HttpPut("{id}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoanDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoanDto>> RejectLoan(Guid id)
        {
            try
            {
                _logger.LogInformation("Rejecting loan with ID: {LoanId}", id);
                var updatedLoan = await _loanService.RejectAsync(id);
                return Ok(updatedLoan);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Loan rejection failed: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error rejecting loan: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        /// <summary>
        /// Deletes a loan.
        /// </summary>
        /// <param name="id">The loan ID to delete.</param>
        /// <returns>No content if successful.</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteLoan(Guid id)
        {
            _logger.LogInformation("Deleting loan with ID: {LoanId}", id);
            var deleted = await _loanService.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Loan with ID {LoanId} not found for deletion", id);
                return NotFound();
            }

            return NoContent();
        }
    }
}
