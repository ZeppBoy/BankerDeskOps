using BankerDeskOps.Application.Jobs;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Jobs
{
    public sealed class SqlJobExecutionStore : IJobExecutionStore
    {
        private readonly AppDbContext _context;

        public SqlJobExecutionStore(AppDbContext context) => _context = context;

        public async Task<bool> TryClaimAsync(string idempotencyKey, string jobName, DateOnly businessDate, CancellationToken cancellationToken = default)
        {
            var execution = new JobExecution
            {
                Id = Guid.NewGuid(),
                JobName = jobName,
                BusinessDate = businessDate,
                StartedAt = DateTime.UtcNow,
                Status = JobExecutionStatus.Running,
                IdempotencyKey = idempotencyKey
            };

            try
            {
                _context.JobExecutions.Add(execution);
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
            {
                _context.Entry(execution).State = EntityState.Detached;
                return false;
            }
        }

        public async Task CompleteAsync(string idempotencyKey, CancellationToken cancellationToken = default)
        {
            var execution = await _context.JobExecutions
                .FirstOrDefaultAsync(e => e.IdempotencyKey == idempotencyKey, cancellationToken);

            if (execution is null) return;

            execution.Status = JobExecutionStatus.Succeeded;
            execution.FinishedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task FailAsync(string idempotencyKey, string errorMessage, CancellationToken cancellationToken = default)
        {
            var execution = await _context.JobExecutions
                .FirstOrDefaultAsync(e => e.IdempotencyKey == idempotencyKey, cancellationToken);

            if (execution is null) return;

            execution.Status = JobExecutionStatus.Failed;
            execution.FinishedAt = DateTime.UtcNow;
            execution.ErrorMessage = errorMessage;
            await _context.SaveChangesAsync(cancellationToken);
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            if (ex.InnerException is SqlException sqlEx)
                return sqlEx.Number == 2627 || sqlEx.Number == 2601;

            // SQLite used in tests
            if (ex.InnerException?.Message?.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase) == true)
                return true;

            return false;
        }
    }
}
