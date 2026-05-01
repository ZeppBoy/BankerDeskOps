using BankerDeskOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankerDeskOps.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DecisionEngineController : ControllerBase
    {
        private readonly IDecisionEngineService _decisionEngineService;

        public DecisionEngineController(IDecisionEngineService decisionEngineService)
        {
            _decisionEngineService = decisionEngineService ?? throw new ArgumentNullException(nameof(decisionEngineService));
        }

        [HttpPost("process-decision")]
        public async Task<IActionResult> ProcessDecision([FromQuery] Guid applicationId)
        {
            var result = await _decisionEngineService.ProcessDecisionAsync(applicationId);
            return Ok(result);
        }

        [HttpPost("auto-approve")]
        public async Task<IActionResult> AutoApprove(
            [FromQuery] Guid applicationId,
            [FromQuery] decimal riskScore)
        {
            var success = await _decisionEngineService.AutoApproveAsync(applicationId, riskScore);
            return success ? Ok(new { message = "Application approved successfully." }) : NotFound();
        }

        [HttpPost("auto-reject")]
        public async Task<IActionResult> AutoReject(
            [FromQuery] Guid applicationId,
            [FromQuery] string reason)
        {
            var success = await _decisionEngineService.AutoRejectAsync(applicationId, reason);
            return success ? Ok(new { message = "Application rejected successfully." }) : NotFound();
        }

        [HttpPost("manual-review")]
        public async Task<IActionResult> MarkForManualReview(
            [FromQuery] Guid applicationId,
            [FromQuery] string reviewReason)
        {
            await _decisionEngineService.MarkForManualReviewAsync(applicationId, reviewReason);
            return Ok(new { message = "Application marked for manual review." });
        }
    }
}
