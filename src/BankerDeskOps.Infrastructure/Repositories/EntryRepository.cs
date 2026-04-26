using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Entry entity data access.
    /// </summary>
    public class EntryRepository : IEntryRepository
    {
        private readonly AppDbContext _context;

        public EntryRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Entry>> GetByAccountIdAsync(Guid accountId)
        {
            return await _context.Entries
                .AsNoTracking()
                .Where(e => e.AccountId == accountId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Entry>> GetByTransactionIdAsync(Guid transactionId)
        {
            return await _context.Entries
                .AsNoTracking()
                .Where(e => e.TransactionId == transactionId)
                .ToListAsync();
        }
    }
}