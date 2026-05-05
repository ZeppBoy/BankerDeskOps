using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankerDeskOps.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoanApplicationsController : ControllerBase
    {
        private readonly ILoanApplicationService _applicationService;
        private readonly ILogger<LoanApplicationsController> _logger;

        public LoanApplicationsController(ILoanApplicationService applicationService, ILogger<LoanApplicationsController> logger)
        {
            _applicationService = applicationService ?? throw new ArgumentNullException(nameof(applicationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<LoanApplicationDto>))]
        public async Task<ActionResult<IEnumerable<LoanApplicationDto>>> GetAllApplications()
        {
            _logger.LogInformation("Fetching all loan applications");
            var applications = await _applicationService.GetAllAsync();
            return Ok(applications);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoanApplicationDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanApplicationDto>> GetApplicationById(Guid id)
        {
            _logger.LogInformation("Fetching loan application with ID: {ApplicationId}", id);
            var application = await _applicationService.GetByIdAsync(id);

            if (application is null)
            {
                _logger.LogWarning("Loan application with ID {ApplicationId} not found", id);
                return NotFound();
            }

            return Ok(application);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(LoanApplicationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoanApplicationDto>> CreateApplication([FromBody] CreateLoanApplicationRequest request)
        {
            if (request is null)
                return BadRequest("Request cannot be null");

            try
            {
                _logger.LogInformation("Creating new loan application for product: {ProductId}", request.ProductId);
                var created = await _applicationService.CreateAsync(request);
                return CreatedAtAction(nameof(GetApplicationById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid loan application creation request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating loan application: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        [HttpPut("{id:guid}/status")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoanApplicationDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LoanApplicationDto>> UpdateStatus(Guid id, [FromBody] object body)
        {
            try
            {
                var status = body.GetType().GetProperty("Status")?.GetValue(body)?.ToString() ?? string.Empty;
                var comment = body.GetType().GetProperty("Comment")?.GetValue(body)?.ToString();

                _logger.LogInformation("Updating loan application {ApplicationId} status to {Status}", id, status);
                var updated = await _applicationService.UpdateStatusAsync(id, status, comment);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Loan application not found: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating loan application status: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteApplication(Guid id)
        {
            _logger.LogInformation("Deleting loan application with ID: {ApplicationId}", id);
            var deleted = await _applicationService.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Loan application with ID {ApplicationId} not found for deletion", id);
                return NotFound();
            }

            return NoContent();
        }
    }
}
