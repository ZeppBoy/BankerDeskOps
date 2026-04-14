using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    /// <summary>
    /// Service interface for loan business logic operations.
    /// </summary>
    public interface ILoanService
    {
        /// <summary>
        /// Retrieves all loans asynchronously.
        /// </summary>
        /// <returns>Collection of loan DTOs.</returns>
        Task<IEnumerable<LoanDto>> GetAllAsync();

        /// <summary>
        /// Retrieves a loan by its identifier.
        /// </summary>
        /// <param name="id">The loan ID.</param>
        /// <returns>The loan DTO if found; otherwise null.</returns>
        Task<LoanDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates a new loan from the request data.
        /// </summary>
        /// <param name="request">The create loan request.</param>
        /// <returns>The created loan DTO.</returns>
        Task<LoanDto> CreateAsync(CreateLoanRequest request);

        /// <summary>
        /// Approves a loan.
        /// </summary>
        /// <param name="id">The loan ID to approve.</param>
        /// <returns>The updated loan DTO.</returns>
        Task<LoanDto> ApproveAsync(Guid id);

        /// <summary>
        /// Rejects a loan.
        /// </summary>
        /// <param name="id">The loan ID to reject.</param>
        /// <returns>The updated loan DTO.</returns>
        Task<LoanDto> RejectAsync(Guid id);

        /// <summary>
        /// Disburses an approved loan — transitions status to <see cref="Domain.Enums.LoanStatus.Disbursed"/>
        /// and atomically creates a <see cref="Domain.Entities.Contract"/> record.
        /// </summary>
        /// <param name="id">The ID of an <c>Approved</c> loan.</param>
        /// <returns>The updated loan DTO with Disbursed status.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the loan is not found or is not in <c>Approved</c> status.
        /// </exception>
        Task<LoanDto> DisburseAsync(Guid id);

        /// <summary>
        /// Deletes a loan.
        /// </summary>
        /// <param name="id">The loan ID to delete.</param>
        /// <returns>True if successful; false if loan not found.</returns>
        Task<bool> DeleteAsync(Guid id);
    }
}
