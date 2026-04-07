using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    /// <summary>
    /// Service interface for retail account business logic operations.
    /// </summary>
    public interface IRetailAccountService
    {
        /// <summary>
        /// Retrieves all retail accounts asynchronously.
        /// </summary>
        /// <returns>Collection of retail account DTOs.</returns>
        Task<IEnumerable<RetailAccountDto>> GetAllAsync();

        /// <summary>
        /// Retrieves a retail account by its identifier.
        /// </summary>
        /// <param name="id">The account ID.</param>
        /// <returns>The account DTO if found; otherwise null.</returns>
        Task<RetailAccountDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Opens a new retail account.
        /// </summary>
        /// <param name="request">The create account request.</param>
        /// <returns>The created account DTO.</returns>
        Task<RetailAccountDto> OpenAsync(CreateRetailAccountRequest request);

        /// <summary>
        /// Deposits funds into an account.
        /// </summary>
        /// <param name="id">The account ID.</param>
        /// <param name="request">The deposit request containing the amount.</param>
        /// <returns>The updated account DTO.</returns>
        Task<RetailAccountDto> DepositAsync(Guid id, DepositRequest request);

        /// <summary>
        /// Withdraws funds from an account.
        /// </summary>
        /// <param name="id">The account ID.</param>
        /// <param name="request">The withdraw request containing the amount.</param>
        /// <returns>The updated account DTO.</returns>
        /// <exception cref="InvalidOperationException">Thrown when insufficient funds.</exception>
        Task<RetailAccountDto> WithdrawAsync(Guid id, WithdrawRequest request);

        /// <summary>
        /// Closes an account.
        /// </summary>
        /// <param name="id">The account ID to close.</param>
        /// <returns>True if successful; false if account not found.</returns>
        Task<bool> CloseAsync(Guid id);
    }
}
