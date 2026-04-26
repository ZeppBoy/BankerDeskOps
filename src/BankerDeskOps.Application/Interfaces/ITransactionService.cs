using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    /// <summary>
    /// Service interface for transaction operations.
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Retrieves all transactions.
        /// </summary>
        Task<IEnumerable<Application.DTOs.TransactionDto>> GetAllAsync();

        /// <summary>
        /// Retrieves a transaction by its identifier with entries.
        /// </summary>
        Task<Application.DTOs.TransactionDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Executes a money transfer between two accounts atomically.
        /// </summary>
        Task<Application.DTOs.TransactionDto> ExecuteTransferAsync(Application.DTOs.TransferRequest request);

        /// <summary>
        /// Retrieves all entries for a specific account.
        /// </summary>
        Task<IEnumerable<Application.DTOs.EntryDto>> GetEntriesByAccountIdAsync(Guid accountId);
    }
}