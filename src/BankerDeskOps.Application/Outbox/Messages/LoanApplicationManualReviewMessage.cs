namespace BankerDeskOps.Application.Outbox.Messages
{
    public record LoanApplicationManualReviewMessage(Guid ApplicationId, string ReviewReason);
}
