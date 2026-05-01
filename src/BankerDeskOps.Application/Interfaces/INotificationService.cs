namespace BankerDeskOps.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendApprovalNotificationAsync(Guid applicationId, string message);
        Task SendRejectionNotificationAsync(Guid applicationId, string reason);
        Task SendManualReviewNotificationAsync(Guid applicationId, string reviewReason);
        Task SendStatusUpdateNotificationAsync(Guid applicationId, string status, string? details = null);
    }
}
