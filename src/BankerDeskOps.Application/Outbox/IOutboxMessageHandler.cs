namespace BankerDeskOps.Application.Outbox
{
    public interface IOutboxMessageHandler
    {
        bool CanHandle(string messageType);
        Task HandleAsync(string messageType, string payloadJson, CancellationToken cancellationToken = default);
    }
}
