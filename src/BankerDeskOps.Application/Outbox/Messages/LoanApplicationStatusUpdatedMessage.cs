namespace BankerDeskOps.Application.Outbox.Messages
{
    public record LoanApplicationStatusUpdatedMessage(Guid ApplicationId, string Status, string? Details);
}
