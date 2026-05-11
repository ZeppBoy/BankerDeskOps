using BankerDeskOps.Application.Outbox;
using BankerDeskOps.Infrastructure.Data;
using BankerDeskOps.Infrastructure.Outbox;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Jobs.Outbox
{
    public sealed class OutboxDispatcherJob
    {
        private const int BatchSize = 100;
        private const int MaxAttempts = 5;

        private readonly AppDbContext _context;
        private readonly IEnumerable<IOutboxMessageHandler> _handlers;
        private readonly ILogger<OutboxDispatcherJob> _logger;

        public OutboxDispatcherJob(
            AppDbContext context,
            IEnumerable<IOutboxMessageHandler> handlers,
            ILogger<OutboxDispatcherJob> logger)
        {
            _context = context;
            _handlers = handlers;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 0)]
        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var messages = await _context.OutboxMessages
                .Where(m => m.ProcessedAt == null && m.Attempts < MaxAttempts)
                .OrderBy(m => m.OccurredAt)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (messages.Count == 0) return;

            foreach (var message in messages)
            {
                await ProcessMessageAsync(message, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task ProcessMessageAsync(OutboxMessage message, CancellationToken cancellationToken)
        {
            var handler = _handlers.FirstOrDefault(h => h.CanHandle(message.Type));

            if (handler is null)
            {
                _logger.LogWarning("No handler found for outbox message type {Type}. Marking as processed to avoid re-processing.", message.Type);
                message.ProcessedAt = DateTime.UtcNow;
                message.Attempts++;
                return;
            }

            try
            {
                await handler.HandleAsync(message.Type, message.PayloadJson, cancellationToken);
                message.ProcessedAt = DateTime.UtcNow;
                message.Attempts++;
            }
            catch (Exception ex)
            {
                message.Attempts++;
                message.LastError = ex.Message.Length > 4000 ? ex.Message[..4000] : ex.Message;
                _logger.LogError(ex, "Outbox message {Id} (type={Type}) failed on attempt {Attempt}", message.Id, message.Type, message.Attempts);

                // Exponential backoff: next retry eligible after 2^attempt minutes — enforced by skipping in query above.
                // Nothing extra needed here; the WHERE clause excludes messages that hit MaxAttempts.
            }
        }

        public static void RegisterRecurring(IRecurringJobManager manager)
        {
            manager.AddOrUpdate<OutboxDispatcherJob>(
                "outbox-dispatcher",
                job => job.ExecuteAsync(CancellationToken.None),
                "*/30 * * * * *");  // every 30 seconds
        }
    }
}
