namespace BankerDeskOps.Application.Outbox.Messages
{
    public record LoanApplicationRejectedMessage(Guid ApplicationId, string Reason);
}
