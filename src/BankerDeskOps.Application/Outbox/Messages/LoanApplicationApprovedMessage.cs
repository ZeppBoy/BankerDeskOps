namespace BankerDeskOps.Application.Outbox.Messages
{
    public record LoanApplicationApprovedMessage(Guid ApplicationId, string Message);
}
