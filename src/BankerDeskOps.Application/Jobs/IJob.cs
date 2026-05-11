namespace BankerDeskOps.Application.Jobs
{
    public interface IJob
    {
        Task ExecuteAsync(JobRunContext context, CancellationToken cancellationToken = default);
    }
}
