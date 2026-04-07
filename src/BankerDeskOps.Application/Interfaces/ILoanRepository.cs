using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    /// <summary>
    /// Repository interface for Loan entity data access operations.
    /// </summary>
    public interface ILoanRepository
    {
        /// <summary>
        /// Retrieves all loans asynchronously.
        /// </summary>
        /// <returns>Collection of all loans.</returns>
        Task<IEnumerable<Loan>> GetAllAsync();

        /// <summary>
        /// Retrieves a loan by its unique identifier.
        /// </summary>
        /// <param name="id">The loan ID.</param>
        /// <returns>The loan if found; otherwise null.</returns>
        Task<Loan?> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates a new loan asynchronously.
        /// </summary>
        /// <param name="loan">The loan entity to create.</param>
        /// <returns>The created loan with generated ID.</returns>
        Task<Loan> CreateAsync(Loan loan);

        /// <summary>
        /// Updates an existing loan asynchronously.
        /// </summary>
        /// <param name="loan">The loan entity with updated values.</param>
        /// <returns>The updated loan.</returns>
        Task<Loan> UpdateAsync(Loan loan);

        /// <summary>
        /// Deletes a loan by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The loan ID to delete.</param>
        /// <returns>True if deletion was successful; false if loan not found.</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
