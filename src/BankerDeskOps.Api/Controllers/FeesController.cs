using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankerDeskOps.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeesController : ControllerBase
    {
        private readonly IFeeService _feeService;
        private readonly ILogger<FeesController> _logger;

        public FeesController(IFeeService feeService, ILogger<FeesController> logger)
        {
            _feeService = feeService ?? throw new ArgumentNullException(nameof(feeService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<FeeDto>))]
        public async Task<ActionResult<IEnumerable<FeeDto>>> GetAllFees()
        {
            _logger.LogInformation("Fetching all fees");
            var fees = await _feeService.GetAllAsync();
            return Ok(fees);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FeeDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FeeDto>> GetFeeById(Guid id)
        {
            _logger.LogInformation("Fetching fee with ID: {FeeId}", id);
            var fee = await _feeService.GetByIdAsync(id);

            if (fee is null)
            {
                _logger.LogWarning("Fee with ID {FeeId} not found", id);
                return NotFound();
            }

            return Ok(fee);
        }

        [HttpGet("product/{productId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<FeeDto>))]
        public async Task<ActionResult<IEnumerable<FeeDto>>> GetFeesByProductId(Guid productId)
        {
            _logger.LogInformation("Fetching fees for product with ID: {ProductId}", productId);
            var fees = await _feeService.GetByProductIdAsync(productId);
            return Ok(fees);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(FeeDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FeeDto>> CreateFee([FromBody] CreateFeeRequest request)
        {
            if (request is null)
                return BadRequest("Request cannot be null");

            try
            {
                _logger.LogInformation("Creating new fee: {Name}", request.Name);
                var created = await _feeService.CreateAsync(request);
                return CreatedAtAction(nameof(GetFeeById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid fee creation request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating fee: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FeeDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FeeDto>> UpdateFee([FromBody] UpdateFeeRequest request)
        {
            if (request is null)
                return BadRequest("Request cannot be null");

            try
            {
                _logger.LogInformation("Updating fee with ID: {FeeId}", request.Id);
                var updated = await _feeService.UpdateAsync(request);
                return Ok(updated);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("Fee not found: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid fee update request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating fee: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFee(Guid id)
        {
            _logger.LogInformation("Deleting fee with ID: {FeeId}", id);
            var deleted = await _feeService.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Fee with ID {FeeId} not found for deletion", id);
                return NotFound();
            }

            return NoContent();
        }
    }
}
