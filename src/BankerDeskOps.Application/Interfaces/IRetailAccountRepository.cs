using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    /// <summary>
    /// Repository interface for RetailAccount entity data access operations.
    /// </summary>
    public interface IRetailAccountRepository
    {
        /// <summary>
        /// Retrieves all retail accounts asynchronously.
        /// </summary>
        /// <returns>Collection of all retail accounts.</returns>
        Task<IEnumerable<RetailAccount>> GetAllAsync();

        /// <summary>
        /// Retrieves a retail account by its unique identifier.
        /// </summary>
        /// <param name="id">The account ID.</param>
        /// <returns>The account if found; otherwise null.</returns>
        Task<RetailAccount?> GetByIdAsync(Guid id);

        /// <summary>
        /// Retrieves a retail account by its account number.
        /// </summary>
        /// <param name="accountNumber">The account number.</param>
        /// <returns>The account if found; otherwise null.</returns>
        Task<RetailAccount?> GetByAccountNumberAsync(string accountNumber);

        /// <summary>
        /// Creates a new retail account asynchronously.
        /// </summary>
        /// <param name="account">The account entity to create.</param>
        /// <returns>The created account with generated ID.</returns>
        Task<RetailAccount> CreateAsync(RetailAccount account);

        /// <summary>
        /// Updates an existing retail account asynchronously.
        /// </summary>
        /// <param name="account">The account entity with updated values.</param>
        /// <returns>The updated account.</returns>
        Task<RetailAccount> UpdateAsync(RetailAccount account);

        /// <summary>
        /// Deletes a retail account by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The account ID to delete.</param>
        /// <returns>True if deletion was successful; false if account not found.</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
