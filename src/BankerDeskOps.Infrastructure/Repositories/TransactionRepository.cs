using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Transaction entity data access.
    /// </summary>
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _context;

        public TransactionRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Transaction>> GetAllAsync()
        {
            return await _context.Transactions
                .AsNoTracking()
                .Include(t => t.Entries)
                .ToListAsync();
        }

        public async Task<Transaction?> GetByIdAsync(Guid id)
        {
            return await _context.Transactions
                .AsNoTracking()
                .Include(t => t.Entries)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Creates a transaction with its entries in a single unit of work.
        /// </summary>
        public async Task<Transaction> CreateWithEntriesAsync(Transaction transaction, ICollection<Entry> entries)
        {
            if (transaction is null)
                throw new ArgumentNullException(nameof(transaction));

            if (entries is null || !entries.Any())
                throw new ArgumentException("Transaction must have at least one entry.", nameof(entries));

            // Add transaction first
            _context.Transactions.Add(transaction);

            // Add all entries linked to this transaction
            foreach (var entry in entries)
            {
                entry.TransactionId = transaction.Id;
                _context.Entries.Add(entry);
            }

            await _context.SaveChangesAsync();

            // Return the transaction with entries loaded
            return await GetByIdAsync(transaction.Id) ?? throw new InvalidOperationException("Failed to retrieve created transaction.");
        }

        /// <summary>
        /// Updates the status of a transaction atomically.
        /// </summary>
        public async Task<bool> UpdateStatusAsync(Guid id, Domain.Enums.TransactionStatus status)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);
            if (transaction is null)
                return false;

            transaction.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}