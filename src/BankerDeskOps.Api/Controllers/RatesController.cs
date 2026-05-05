using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankerDeskOps.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatesController : ControllerBase
    {
        private readonly IRateService _rateService;
        private readonly ILogger<RatesController> _logger;

        public RatesController(IRateService rateService, ILogger<RatesController> logger)
        {
            _rateService = rateService ?? throw new ArgumentNullException(nameof(rateService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RateDto>))]
        public async Task<ActionResult<IEnumerable<RateDto>>> GetAllRates()
        {
            _logger.LogInformation("Fetching all rates");
            var rates = await _rateService.GetAllAsync();
            return Ok(rates);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RateDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RateDto>> GetRateById(Guid id)
        {
            _logger.LogInformation("Fetching rate with ID: {RateId}", id);
            var rate = await _rateService.GetByIdAsync(id);

            if (rate is null)
            {
                _logger.LogWarning("Rate with ID {RateId} not found", id);
                return NotFound();
            }

            return Ok(rate);
        }

        [HttpGet("product/{productId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RateDto>))]
        public async Task<ActionResult<IEnumerable<RateDto>>> GetRatesByProductId(Guid productId)
        {
            _logger.LogInformation("Fetching rates for product with ID: {ProductId}", productId);
            var rates = await _rateService.GetByProductIdAsync(productId);
            return Ok(rates);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RateDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RateDto>> CreateRate([FromBody] CreateRateRequest request)
        {
            if (request is null)
                return BadRequest("Request cannot be null");

            try
            {
                _logger.LogInformation("Creating new rate for product: {ProductId}", request.ProductId);
                var created = await _rateService.CreateAsync(request);
                return CreatedAtAction(nameof(GetRateById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid rate creation request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating rate: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RateDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RateDto>> UpdateRate([FromBody] UpdateRateRequest request)
        {
            if (request is null)
                return BadRequest("Request cannot be null");

            try
            {
                _logger.LogInformation("Updating rate with ID: {RateId}", request.Id);
                var updated = await _rateService.UpdateAsync(request);
                return Ok(updated);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("Rate not found: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid rate update request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating rate: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRate(Guid id)
        {
            _logger.LogInformation("Deleting rate with ID: {RateId}", id);
            var deleted = await _rateService.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Rate with ID {RateId} not found for deletion", id);
                return NotFound();
            }

            return NoContent();
        }
    }
}
