using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    /// <summary>
    /// Repository interface for transaction operations.
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>
        /// Retrieves all transactions.
        /// </summary>
        Task<IEnumerable<Transaction>> GetAllAsync();

        /// <summary>
        /// Retrieves a transaction by its identifier.
        /// </summary>
        Task<Transaction?> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates a new transaction with entries in a single unit of work.
        /// </summary>
        Task<Transaction> CreateWithEntriesAsync(Transaction transaction, ICollection<Entry> entries);

        /// <summary>
        /// Updates the status of a transaction and all related entries atomically.
        /// </summary>
        Task<bool> UpdateStatusAsync(Guid id, Domain.Enums.TransactionStatus status);
    }
}