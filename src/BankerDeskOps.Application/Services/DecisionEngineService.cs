using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Services
{
    public class DecisionEngineService : IDecisionEngineService
    {
        private readonly ILoanApplicationRepository _applicationRepository;
        private readonly IAIAnalysisService _aiAnalysisService;
        private readonly ILoanValidationService _validationService;
        private readonly INotificationService _notificationService;

        public DecisionEngineService(
            ILoanApplicationRepository applicationRepository,
            IAIAnalysisService aiAnalysisService,
            ILoanValidationService validationService,
            INotificationService notificationService)
        {
            _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
            _aiAnalysisService = aiAnalysisService ?? throw new ArgumentNullException(nameof(aiAnalysisService));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        public async Task<DecisionResultDto> ProcessDecisionAsync(Guid applicationId)
        {
            var validationResult = await _validationService.ValidateApplicationAsync(applicationId);
            if (!validationResult.IsValid)
            {
                await AutoRejectAsync(applicationId, validationResult.ErrorMessage ?? "Validation failed.");
                return new DecisionResultDto
                {
                    ApplicationId = applicationId,
                    Status = LoanApplicationStatus.Rejected,
                    Reason = validationResult.ErrorMessage ?? "Validation failed.",
                    DecidedAt = DateTime.UtcNow,
                    IsAutomated = true
                };
            }

            var aiAnalysis = await _aiAnalysisService.AnalyzeApplicationAsync(applicationId);

            if (aiAnalysis.RiskScore >= 0.8m)
            {
                await AutoApproveAsync(applicationId, aiAnalysis.RiskScore);
                return new DecisionResultDto
                {
                    ApplicationId = applicationId,
                    Status = LoanApplicationStatus.Approved,
                    Reason = $"Auto-approved with risk score {aiAnalysis.RiskScore:P2}.",
                    DecidedAt = DateTime.UtcNow,
                    IsAutomated = true
                };
            }
            else if (aiAnalysis.RiskScore < 0.5m)
            {
                await AutoRejectAsync(applicationId, "Risk score below acceptable threshold.");
                return new DecisionResultDto
                {
                    ApplicationId = applicationId,
                    Status = LoanApplicationStatus.Rejected,
                    Reason = "Risk score below acceptable threshold.",
                    DecidedAt = DateTime.UtcNow,
                    IsAutomated = true
                };
            }
            else
            {
                await MarkForManualReviewAsync(applicationId, "Application requires manual review due to medium risk level.");
                return new DecisionResultDto
                {
                    ApplicationId = applicationId,
                    Status = LoanApplicationStatus.UnderReview,
                    Reason = "Marked for manual review.",
                    DecidedAt = DateTime.UtcNow,
                    IsAutomated = false
                };
            }
        }

        public async Task<bool> AutoApproveAsync(Guid applicationId, decimal riskScore)
        {
            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null) return false;

            application.Status = LoanApplicationStatus.Approved;
            application.ApprovedDate = DateTime.UtcNow;

            await _applicationRepository.UpdateAsync(application);
            await _notificationService.SendApprovalNotificationAsync(applicationId, $"Loan approved with risk score {riskScore:P2}.");

            return true;
        }

        public async Task<bool> AutoRejectAsync(Guid applicationId, string reason)
        {
            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null) return false;

            application.Status = LoanApplicationStatus.Rejected;
            application.RejectedDate = DateTime.UtcNow;

            await _applicationRepository.UpdateAsync(application);
            await _notificationService.SendRejectionNotificationAsync(applicationId, reason);

            return true;
        }

        public async Task MarkForManualReviewAsync(Guid applicationId, string reviewReason)
        {
            var application = await _applicationRepository.GetByIdAsync(applicationId);
            if (application == null) return;

            application.Status = LoanApplicationStatus.UnderReview;

            await _applicationRepository.UpdateAsync(application);
            await _notificationService.SendManualReviewNotificationAsync(applicationId, reviewReason);
        }
    }
}
