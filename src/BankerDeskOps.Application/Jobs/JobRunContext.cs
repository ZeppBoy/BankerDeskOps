namespace BankerDeskOps.Application.Jobs
{
    public record JobRunContext(Guid RunId, DateOnly BusinessDate, bool DryRun);
}
