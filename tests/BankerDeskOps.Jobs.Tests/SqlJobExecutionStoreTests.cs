using BankerDeskOps.Infrastructure.Data;
using BankerDeskOps.Infrastructure.Jobs;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Jobs.Tests
{
    public class SqlJobExecutionStoreTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly SqlJobExecutionStore _store;

        public SqlJobExecutionStoreTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"DataSource=:memory:")
                .Options;

            _context = new AppDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();
            _store = new SqlJobExecutionStore(_context);
        }

        [Fact]
        public async Task TryClaimAsync_FirstCall_ReturnsTrue()
        {
            var result = await _store.TryClaimAsync("test-key-1", "TestJob", new DateOnly(2026, 5, 11));

            Assert.True(result);
        }

        [Fact]
        public async Task TryClaimAsync_SecondCallWithSameKey_ReturnsFalse()
        {
            await _store.TryClaimAsync("test-key-2", "TestJob", new DateOnly(2026, 5, 11));
            var second = await _store.TryClaimAsync("test-key-2", "TestJob", new DateOnly(2026, 5, 11));

            Assert.False(second);
        }

        [Fact]
        public async Task TryClaimAsync_DifferentKeys_BothSucceed()
        {
            var first = await _store.TryClaimAsync("key-a", "TestJob", new DateOnly(2026, 5, 11));
            var second = await _store.TryClaimAsync("key-b", "TestJob", new DateOnly(2026, 5, 11));

            Assert.True(first);
            Assert.True(second);
        }

        [Fact]
        public async Task CompleteAsync_SetsStatusSucceeded()
        {
            await _store.TryClaimAsync("complete-key", "TestJob", new DateOnly(2026, 5, 11));
            await _store.CompleteAsync("complete-key");

            var row = await _context.JobExecutions.FirstAsync(e => e.IdempotencyKey == "complete-key");
            Assert.Equal(JobExecutionStatus.Succeeded, row.Status);
            Assert.NotNull(row.FinishedAt);
        }

        [Fact]
        public async Task FailAsync_SetsStatusFailedWithMessage()
        {
            await _store.TryClaimAsync("fail-key", "TestJob", new DateOnly(2026, 5, 11));
            await _store.FailAsync("fail-key", "Something went wrong");

            var row = await _context.JobExecutions.FirstAsync(e => e.IdempotencyKey == "fail-key");
            Assert.Equal(JobExecutionStatus.Failed, row.Status);
            Assert.Equal("Something went wrong", row.ErrorMessage);
            Assert.NotNull(row.FinishedAt);
        }

        public void Dispose()
        {
            _context.Database.CloseConnection();
            _context.Dispose();
        }
    }
}
