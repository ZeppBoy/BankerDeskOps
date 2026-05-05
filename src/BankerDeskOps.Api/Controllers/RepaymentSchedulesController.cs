using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankerDeskOps.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RepaymentSchedulesController : ControllerBase
    {
        private readonly IRepaymentScheduleService _scheduleService;
        private readonly ILogger<RepaymentSchedulesController> _logger;

        public RepaymentSchedulesController(IRepaymentScheduleService scheduleService, ILogger<RepaymentSchedulesController> logger)
        {
            _scheduleService = scheduleService ?? throw new ArgumentNullException(nameof(scheduleService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RepaymentScheduleDto>))]
        public async Task<ActionResult<IEnumerable<RepaymentScheduleDto>>> GetAllSchedules()
        {
            _logger.LogInformation("Fetching all repayment schedules");
            var schedules = await _scheduleService.GetAllAsync();
            return Ok(schedules);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RepaymentScheduleDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RepaymentScheduleDto>> GetScheduleById(Guid id)
        {
            _logger.LogInformation("Fetching repayment schedule with ID: {ScheduleId}", id);
            var schedule = await _scheduleService.GetByIdAsync(id);

            if (schedule is null)
            {
                _logger.LogWarning("Repayment schedule with ID {ScheduleId} not found", id);
                return NotFound();
            }

            return Ok(schedule);
        }

        [HttpGet("by-application/{loanApplicationId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<RepaymentScheduleDto>))]
        public async Task<ActionResult<IEnumerable<RepaymentScheduleDto>>> GetSchedulesByLoanApplication(Guid loanApplicationId)
        {
            _logger.LogInformation("Fetching repayment schedules for loan application: {LoanApplicationId}", loanApplicationId);
            var schedules = await _scheduleService.GetByLoanApplicationIdAsync(loanApplicationId);
            return Ok(schedules);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(RepaymentScheduleDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RepaymentScheduleDto>> CreateSchedule([FromBody] CreateRepaymentScheduleRequest request)
        {
            if (request is null)
                return BadRequest("Request cannot be null");

            try
            {
                _logger.LogInformation("Creating new repayment schedule for application: {LoanApplicationId}", request.LoanApplicationId);
                var created = await _scheduleService.CreateAsync(request);
                return CreatedAtAction(nameof(GetScheduleById), new { id = created.ScheduleId }, created);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid schedule creation request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating repayment schedule: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSchedule(Guid id)
        {
            _logger.LogInformation("Deleting repayment schedule with ID: {ScheduleId}", id);
            var deleted = await _scheduleService.DeleteAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Repayment schedule with ID {ScheduleId} not found for deletion", id);
                return NotFound();
            }

            return NoContent();
        }
    }
}
