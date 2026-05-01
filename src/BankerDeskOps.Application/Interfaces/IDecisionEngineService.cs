using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    public interface IDecisionEngineService
    {
        Task<DecisionResultDto> ProcessDecisionAsync(Guid applicationId);
        Task<bool> AutoApproveAsync(Guid applicationId, decimal riskScore);
        Task<bool> AutoRejectAsync(Guid applicationId, string reason);
        Task MarkForManualReviewAsync(Guid applicationId, string reviewReason);
    }

    public class DecisionResultDto
    {
        public Guid ApplicationId { get; set; }
        public LoanApplicationStatus Status { get; set; }
        public string? Reason { get; set; }
        public DateTime DecidedAt { get; set; }
        public bool IsAutomated { get; set; }
    }
}
