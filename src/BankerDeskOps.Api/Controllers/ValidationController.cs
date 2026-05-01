using BankerDeskOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankerDeskOps.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValidationController : ControllerBase
    {
        private readonly ILoanValidationService _validationService;

        public ValidationController(ILoanValidationService validationService)
        {
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        }

        [HttpPost("validate-application")]
        public async Task<IActionResult> ValidateApplication([FromQuery] Guid applicationId)
        {
            var result = await _validationService.ValidateApplicationAsync(applicationId);
            return Ok(result);
        }

        [HttpPost("validate-loan-parameters")]
        public IActionResult ValidateLoanParameters(
            [FromQuery] decimal amount,
            [FromQuery] int termMonths)
        {
            var result = _validationService.ValidateAmount(amount, termMonths);
            return Ok(result);
        }

        [HttpPost("validate-client-eligibility")]
        public IActionResult ValidateClientEligibility(
            [FromQuery] string clientType,
            [FromQuery] decimal annualIncome,
            [FromQuery] decimal loanAmount)
        {
            var result = _validationService.ValidateClientEligibility(clientType, annualIncome, loanAmount);
            return Ok(result);
        }

        [HttpPost("validate-dti-ratio")]
        public IActionResult ValidateDTIRatio(
            [FromQuery] decimal monthlyDebtPayments,
            [FromQuery] decimal monthlyIncome)
        {
            var result = _validationService.ValidateDebtToIncomeRatio(monthlyDebtPayments, monthlyIncome);
            return Ok(result);
        }
    }
}
