namespace BankerDeskOps.Application.Jobs
{
    public interface IJobExecutionStore
    {
        Task<bool> TryClaimAsync(string idempotencyKey, string jobName, DateOnly businessDate, CancellationToken cancellationToken = default);
        Task CompleteAsync(string idempotencyKey, CancellationToken cancellationToken = default);
        Task FailAsync(string idempotencyKey, string errorMessage, CancellationToken cancellationToken = default);
    }
}
