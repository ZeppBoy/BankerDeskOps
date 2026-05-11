using System.Text.Json;
using BankerDeskOps.Application.Outbox;
using BankerDeskOps.Infrastructure.Data;

namespace BankerDeskOps.Infrastructure.Outbox
{
    public sealed class OutboxWriter : IOutboxWriter
    {
        private readonly AppDbContext _context;

        public OutboxWriter(AppDbContext context) => _context = context;

        public async Task EnqueueAsync<T>(T payload, CancellationToken cancellationToken = default) where T : notnull
        {
            var message = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                Type = typeof(T).FullName ?? typeof(T).Name,
                PayloadJson = JsonSerializer.Serialize(payload)
            };

            _context.OutboxMessages.Add(message);
            // Caller is responsible for calling SaveChangesAsync — outbox participates in the caller's TX.
        }
    }
}
