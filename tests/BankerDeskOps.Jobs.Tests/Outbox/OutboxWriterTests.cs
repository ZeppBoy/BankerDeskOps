using BankerDeskOps.Application.Outbox.Messages;
using BankerDeskOps.Infrastructure.Data;
using BankerDeskOps.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BankerDeskOps.Jobs.Tests.Outbox
{
    public class OutboxWriterTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly OutboxWriter _writer;

        public OutboxWriterTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _context = new AppDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();
            _writer = new OutboxWriter(_context);
        }

        [Fact]
        public async Task EnqueueAsync_WritesRowToContext()
        {
            await _writer.EnqueueAsync(new LoanApplicationApprovedMessage(Guid.NewGuid(), "Approved!"));
            await _context.SaveChangesAsync();

            var count = await _context.OutboxMessages.CountAsync();
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task EnqueueAsync_StoresCorrectType()
        {
            var expectedType = typeof(LoanApplicationApprovedMessage).FullName!;
            await _writer.EnqueueAsync(new LoanApplicationApprovedMessage(Guid.NewGuid(), "Test"));
            await _context.SaveChangesAsync();

            var message = await _context.OutboxMessages.SingleAsync();
            Assert.Equal(expectedType, message.Type);
        }

        [Fact]
        public async Task EnqueueAsync_RolledBackTransaction_LeavesZeroRows()
        {
            // Outbox row shares the same transaction as the caller — rollback removes both.
            await using var tx = await _context.Database.BeginTransactionAsync();

            await _writer.EnqueueAsync(new LoanApplicationApprovedMessage(Guid.NewGuid(), "Will roll back"));
            await _context.SaveChangesAsync();

            await tx.RollbackAsync();

            var count = await _context.OutboxMessages.CountAsync();
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task EnqueueAsync_UnprocessedRowHasNullProcessedAt()
        {
            await _writer.EnqueueAsync(new LoanApplicationRejectedMessage(Guid.NewGuid(), "Denied"));
            await _context.SaveChangesAsync();

            var message = await _context.OutboxMessages.SingleAsync();
            Assert.Null(message.ProcessedAt);
            Assert.Equal(0, message.Attempts);
        }

        public void Dispose()
        {
            _context.Database.CloseConnection();
            _context.Dispose();
        }
    }
}
