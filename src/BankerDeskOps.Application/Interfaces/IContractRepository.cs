using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    /// <summary>
    /// Repository interface for Contract entity data access operations.
    /// </summary>
    public interface IContractRepository
    {
        Task<IEnumerable<Contract>> GetAllAsync();
        Task<Contract?> GetByIdAsync(Guid id);

        /// <summary>Looks up the single contract associated with a given loan.</summary>
        Task<Contract?> GetByLoanIdAsync(Guid loanId);

        Task<Contract?> GetByContractNumberAsync(string contractNumber);
        Task<Contract> CreateAsync(Contract contract);
        Task<Contract> UpdateAsync(Contract contract);
        Task<bool> DeleteAsync(Guid id);
    }
}
