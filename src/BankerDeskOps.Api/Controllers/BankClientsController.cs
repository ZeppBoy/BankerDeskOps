using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankerDeskOps.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankClientsController : ControllerBase
    {
        private readonly IBankClientService _clientService;
        private readonly ILogger<BankClientsController> _logger;

        public BankClientsController(IBankClientService clientService, ILogger<BankClientsController> logger)
        {
            _clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BankClientDto>))]
        public async Task<ActionResult<IEnumerable<BankClientDto>>> GetAllClients()
        {
            _logger.LogInformation("Fetching all bank clients");
            var clients = await _clientService.GetAllAsync();
            return Ok(clients);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BankClientDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BankClientDto>> GetClientById(Guid id)
        {
            _logger.LogInformation("Fetching bank client with ID: {ClientId}", id);
            var client = await _clientService.GetByIdAsync(id);

            if (client is null)
            {
                _logger.LogWarning("Bank client with ID {ClientId} not found", id);
                return NotFound();
            }

            return Ok(client);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BankClientDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BankClientDto>> CreateClient([FromBody] CreateBankClientRequest request)
        {
            if (request is null)
                return BadRequest("Request cannot be null");

            try
            {
                _logger.LogInformation("Creating new bank client: {Email}", request.Email);
                var created = await _clientService.CreateAsync(request);
                return CreatedAtAction(nameof(GetClientById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid bank client creation request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Bank client creation conflict: {Message}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating bank client: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BankClientDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BankClientDto>> UpdateClient(Guid id, [FromBody] UpdateBankClientRequest request)
        {
            if (request is null)
                return BadRequest("Request cannot be null");

            try
            {
                _logger.LogInformation("Updating bank client with ID: {ClientId}", id);
                var updated = await _clientService.UpdateAsync(id, request);
                return Ok(updated);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid bank client update request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                _logger.LogWarning("Bank client not found: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Bank client update conflict: {Message}", ex.Message);
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating bank client: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        [HttpPut("{id:guid}/suspend")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BankClientDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BankClientDto>> SuspendClient(Guid id)
        {
            try
            {
                _logger.LogInformation("Suspending bank client with ID: {ClientId}", id);
                var updated = await _clientService.SuspendAsync(id);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Suspend client failed: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error suspending bank client: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        [HttpPut("{id:guid}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BankClientDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BankClientDto>> ActivateClient(Guid id)
        {
            try
            {
                _logger.LogInformation("Activating bank client with ID: {ClientId}", id);
                var updated = await _clientService.ActivateAsync(id);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Activate client failed: {Message}", ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error activating bank client: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            _logger.LogInformation("Deleting bank client with ID: {ClientId}", id);
            var deleted = await _clientService.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Bank client with ID {ClientId} not found for deletion", id);
                return NotFound();
            }

            return NoContent();
        }
    }
}
