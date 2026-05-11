namespace BankerDeskOps.Application.Outbox
{
    public interface IOutboxWriter
    {
        Task EnqueueAsync<T>(T payload, CancellationToken cancellationToken = default) where T : notnull;
    }
}
