using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankerDeskOps.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommissionsController : ControllerBase
    {
        private readonly ICommissionService _commissionService;
        private readonly ILogger<CommissionsController> _logger;

        public CommissionsController(ICommissionService commissionService, ILogger<CommissionsController> logger)
        {
            _commissionService = commissionService ?? throw new ArgumentNullException(nameof(commissionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CommissionDto>))]
        public async Task<ActionResult<IEnumerable<CommissionDto>>> GetAllCommissions()
        {
            _logger.LogInformation("Fetching all commissions");
            var commissions = await _commissionService.GetAllAsync();
            return Ok(commissions);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CommissionDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommissionDto>> GetCommissionById(Guid id)
        {
            _logger.LogInformation("Fetching commission with ID: {CommissionId}", id);
            var commission = await _commissionService.GetByIdAsync(id);

            if (commission is null)
            {
                _logger.LogWarning("Commission with ID {CommissionId} not found", id);
                return NotFound();
            }

            return Ok(commission);
        }

        [HttpGet("product/{productId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CommissionDto>))]
        public async Task<ActionResult<IEnumerable<CommissionDto>>> GetCommissionsByProductId(Guid productId)
        {
            _logger.LogInformation("Fetching commissions for product with ID: {ProductId}", productId);
            var commissions = await _commissionService.GetByProductIdAsync(productId);
            return Ok(commissions);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CommissionDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CommissionDto>> CreateCommission([FromBody] CreateCommissionRequest request)
        {
            if (request is null)
                return BadRequest("Request cannot be null");

            try
            {
                _logger.LogInformation("Creating new commission: {Name}", request.Name);
                var created = await _commissionService.CreateAsync(request);
                return CreatedAtAction(nameof(GetCommissionById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid commission creation request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating commission: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CommissionDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CommissionDto>> UpdateCommission([FromBody] UpdateCommissionRequest request)
        {
            if (request is null)
                return BadRequest("Request cannot be null");

            try
            {
                _logger.LogInformation("Updating commission with ID: {CommissionId}", request.Id);
                var updated = await _commissionService.UpdateAsync(request);
                return Ok(updated);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("Commission not found: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid commission update request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating commission: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCommission(Guid id)
        {
            _logger.LogInformation("Deleting commission with ID: {CommissionId}", id);
            var deleted = await _commissionService.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Commission with ID {CommissionId} not found for deletion", id);
                return NotFound();
            }

            return NoContent();
        }
    }
}
