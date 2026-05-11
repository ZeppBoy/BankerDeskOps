using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Application.Outbox;
using BankerDeskOps.Application.Outbox.Messages;

namespace BankerDeskOps.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IOutboxWriter _outbox;

        public NotificationService(IOutboxWriter outbox) => _outbox = outbox;

        public Task SendApprovalNotificationAsync(Guid applicationId, string message)
            => _outbox.EnqueueAsync(new LoanApplicationApprovedMessage(applicationId, message));

        public Task SendRejectionNotificationAsync(Guid applicationId, string reason)
            => _outbox.EnqueueAsync(new LoanApplicationRejectedMessage(applicationId, reason));

        public Task SendManualReviewNotificationAsync(Guid applicationId, string reviewReason)
            => _outbox.EnqueueAsync(new LoanApplicationManualReviewMessage(applicationId, reviewReason));

        public Task SendStatusUpdateNotificationAsync(Guid applicationId, string status, string? details = null)
            => _outbox.EnqueueAsync(new LoanApplicationStatusUpdatedMessage(applicationId, status, details));
    }
}
