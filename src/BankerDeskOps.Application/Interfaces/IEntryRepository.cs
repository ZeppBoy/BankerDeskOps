using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    /// <summary>
    /// Repository interface for entry operations.
    /// </summary>
    public interface IEntryRepository
    {
        /// <summary>
        /// Retrieves all entries for a specific account.
        /// </summary>
        Task<IEnumerable<Entry>> GetByAccountIdAsync(Guid accountId);

        /// <summary>
        /// Retrieves all entries for a specific transaction.
        /// </summary>
        Task<IEnumerable<Entry>> GetByTransactionIdAsync(Guid transactionId);
    }
}