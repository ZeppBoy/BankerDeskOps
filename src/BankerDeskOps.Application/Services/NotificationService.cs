using BankerDeskOps.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task SendApprovalNotificationAsync(Guid applicationId, string message)
        {
            _logger.LogInformation("Loan Application {ApplicationId} has been APPROVED. Message: {Message}", applicationId, message);
            return Task.CompletedTask;
        }

        public Task SendRejectionNotificationAsync(Guid applicationId, string reason)
        {
            _logger.LogWarning("Loan Application {ApplicationId} has been REJECTED. Reason: {Reason}", applicationId, reason);
            return Task.CompletedTask;
        }

        public Task SendManualReviewNotificationAsync(Guid applicationId, string reviewReason)
        {
            _logger.LogInformation("Loan Application {ApplicationId} marked for MANUAL REVIEW. Reason: {ReviewReason}", applicationId, reviewReason);
            return Task.CompletedTask;
        }

        public Task SendStatusUpdateNotificationAsync(Guid applicationId, string status, string? details = null)
        {
            _logger.LogInformation(
                "Loan Application {ApplicationId} status updated to {Status}. Details: {Details}",
                applicationId, status, details ?? "N/A");
            return Task.CompletedTask;
        }
    }
}
