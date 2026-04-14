using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    /// <summary>
    /// Service interface for contract read operations.
    /// Contract creation is handled exclusively by
    /// <see cref="ILoanService.DisburseAsync"/> to enforce the invariant
    /// that every contract originates from a disbursed loan.
    /// </summary>
    public interface IContractService
    {
        Task<IEnumerable<ContractDto>> GetAllAsync();
        Task<ContractDto?> GetByIdAsync(Guid id);

        /// <summary>Returns the contract associated with the specified loan, or null.</summary>
        Task<ContractDto?> GetByLoanIdAsync(Guid loanId);
    }
}
