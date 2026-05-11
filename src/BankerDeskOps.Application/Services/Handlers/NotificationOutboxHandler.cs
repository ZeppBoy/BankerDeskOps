using System.Text.Json;
using BankerDeskOps.Application.Outbox;
using BankerDeskOps.Application.Outbox.Messages;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Application.Services.Handlers
{
    public sealed class NotificationOutboxHandler : IOutboxMessageHandler
    {
        private readonly ILogger<NotificationOutboxHandler> _logger;

        private static readonly HashSet<string> _handledTypes = new()
        {
            typeof(LoanApplicationApprovedMessage).FullName!,
            typeof(LoanApplicationRejectedMessage).FullName!,
            typeof(LoanApplicationManualReviewMessage).FullName!,
            typeof(LoanApplicationStatusUpdatedMessage).FullName!
        };

        public NotificationOutboxHandler(ILogger<NotificationOutboxHandler> logger) => _logger = logger;

        public bool CanHandle(string messageType) => _handledTypes.Contains(messageType);

        public Task HandleAsync(string messageType, string payloadJson, CancellationToken cancellationToken = default)
        {
            if (messageType == typeof(LoanApplicationApprovedMessage).FullName)
            {
                var msg = JsonSerializer.Deserialize<LoanApplicationApprovedMessage>(payloadJson)!;
                _logger.LogInformation("NOTIFICATION [Approved] ApplicationId={Id} Message={Msg}", msg.ApplicationId, msg.Message);
            }
            else if (messageType == typeof(LoanApplicationRejectedMessage).FullName)
            {
                var msg = JsonSerializer.Deserialize<LoanApplicationRejectedMessage>(payloadJson)!;
                _logger.LogWarning("NOTIFICATION [Rejected] ApplicationId={Id} Reason={Reason}", msg.ApplicationId, msg.Reason);
            }
            else if (messageType == typeof(LoanApplicationManualReviewMessage).FullName)
            {
                var msg = JsonSerializer.Deserialize<LoanApplicationManualReviewMessage>(payloadJson)!;
                _logger.LogInformation("NOTIFICATION [ManualReview] ApplicationId={Id} Reason={Reason}", msg.ApplicationId, msg.ReviewReason);
            }
            else if (messageType == typeof(LoanApplicationStatusUpdatedMessage).FullName)
            {
                var msg = JsonSerializer.Deserialize<LoanApplicationStatusUpdatedMessage>(payloadJson)!;
                _logger.LogInformation("NOTIFICATION [StatusUpdated] ApplicationId={Id} Status={Status} Details={Details}", msg.ApplicationId, msg.Status, msg.Details ?? "N/A");
            }

            return Task.CompletedTask;
        }
    }
}
