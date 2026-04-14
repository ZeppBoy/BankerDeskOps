using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankerDeskOps.Api.Controllers
{
    /// <summary>
    /// API controller for querying loan contracts.
    /// Contracts are created automatically via PUT /api/loans/{id}/disburse.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(IContractService contractService, ILogger<ContractsController> logger)
        {
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>Retrieves all contracts.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ContractDto>))]
        public async Task<ActionResult<IEnumerable<ContractDto>>> GetAllContracts()
        {
            _logger.LogInformation("Fetching all contracts");
            var contracts = await _contractService.GetAllAsync();
            return Ok(contracts);
        }

        /// <summary>Retrieves a contract by its ID.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ContractDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContractDto>> GetContractById(Guid id)
        {
            _logger.LogInformation("Fetching contract {ContractId}", id);
            var contract = await _contractService.GetByIdAsync(id);

            if (contract is null)
            {
                _logger.LogWarning("Contract {ContractId} not found", id);
                return NotFound();
            }

            return Ok(contract);
        }

        /// <summary>Retrieves the contract associated with a specific loan.</summary>
        [HttpGet("by-loan/{loanId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ContractDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContractDto>> GetContractByLoanId(Guid loanId)
        {
            _logger.LogInformation("Fetching contract for loan {LoanId}", loanId);
            var contract = await _contractService.GetByLoanIdAsync(loanId);

            if (contract is null)
            {
                _logger.LogWarning("No contract found for loan {LoanId}", loanId);
                return NotFound();
            }

            return Ok(contract);
        }
    }
}
