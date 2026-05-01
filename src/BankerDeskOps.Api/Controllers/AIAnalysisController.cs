using BankerDeskOps.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankerDeskOps.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AIAnalysisController : ControllerBase
    {
        private readonly IAIAnalysisService _aiAnalysisService;

        public AIAnalysisController(IAIAnalysisService aiAnalysisService)
        {
            _aiAnalysisService = aiAnalysisService ?? throw new ArgumentNullException(nameof(aiAnalysisService));
        }

        [HttpPost("analyze-application")]
        public async Task<IActionResult> AnalyzeApplication([FromQuery] Guid applicationId)
        {
            try
            {
                var result = await _aiAnalysisService.AnalyzeApplicationAsync(applicationId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost("analyze-document")]
        public async Task<IActionResult> AnalyzeDocument([FromQuery] Guid documentId)
        {
            try
            {
                var result = await _aiAnalysisService.AnalyzeDocumentAsync(documentId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost("assess-risk")]
        public async Task<IActionResult> AssessRisk([FromBody] BankerDeskOps.Application.Interfaces.RiskAssessmentRequest request)
        {
            if (request == null) return BadRequest();

            var result = await _aiAnalysisService.AssessRiskAsync(request);
            return Ok(result);
        }
    }
}
