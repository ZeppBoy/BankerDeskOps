using BankerDeskOps.Application.Outbox;
using BankerDeskOps.Application.Outbox.Messages;
using BankerDeskOps.Infrastructure.Data;
using BankerDeskOps.Infrastructure.Outbox;
using BankerDeskOps.Jobs.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace BankerDeskOps.Jobs.Tests.Outbox
{
    public class OutboxDispatcherJobTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly OutboxDispatcherJob _job;
        private readonly IOutboxMessageHandler _handler;

        public OutboxDispatcherJobTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _context = new AppDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();

            _handler = Substitute.For<IOutboxMessageHandler>();
            _handler.CanHandle(Arg.Any<string>()).Returns(true);
            _handler.HandleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            _job = new OutboxDispatcherJob(_context, [_handler], NullLogger<OutboxDispatcherJob>.Instance);
        }

        private async Task SeedMessageAsync(string type = "TestType", bool alreadyProcessed = false, int attempts = 0)
        {
            _context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredAt = DateTime.UtcNow,
                Type = type,
                PayloadJson = "{}",
                ProcessedAt = alreadyProcessed ? DateTime.UtcNow : null,
                Attempts = attempts
            });
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task ExecuteAsync_ProcessesUnprocessedMessages()
        {
            await SeedMessageAsync();

            await _job.ExecuteAsync();

            var message = await _context.OutboxMessages.SingleAsync();
            Assert.NotNull(message.ProcessedAt);
        }

        [Fact]
        public async Task ExecuteAsync_SkipsAlreadyProcessedMessages()
        {
            await SeedMessageAsync(alreadyProcessed: true);

            await _job.ExecuteAsync();

            await _handler.DidNotReceive().HandleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ExecuteAsync_FailureIncrementsAttempts()
        {
            await SeedMessageAsync();
            _handler.HandleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromException(new InvalidOperationException("boom")));

            await _job.ExecuteAsync();

            var message = await _context.OutboxMessages.SingleAsync();
            Assert.Equal(1, message.Attempts);
            Assert.Equal("boom", message.LastError);
            Assert.Null(message.ProcessedAt);
        }

        [Fact]
        public async Task ExecuteAsync_SkipsMessagesAtMaxAttempts()
        {
            await SeedMessageAsync(attempts: 5);

            await _job.ExecuteAsync();

            await _handler.DidNotReceive().HandleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ExecuteAsync_ProcessesOnlyUnprocessed_WhenMixed()
        {
            await SeedMessageAsync("Type1", alreadyProcessed: false);
            await SeedMessageAsync("Type2", alreadyProcessed: true);

            await _job.ExecuteAsync();

            await _handler.Received(1).HandleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        public void Dispose()
        {
            _context.Database.CloseConnection();
            _context.Dispose();
        }
    }
}
