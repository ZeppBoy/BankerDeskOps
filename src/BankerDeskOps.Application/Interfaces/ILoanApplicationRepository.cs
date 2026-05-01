using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    public interface ILoanApplicationRepository
    {
        Task<IEnumerable<LoanApplication>> GetAllAsync();
        Task<LoanApplication?> GetByIdAsync(Guid id);
        Task<LoanApplication?> GetByRequestIdAsync(string requestId);
        Task<IEnumerable<LoanApplication>> GetByProductIdAsync(Guid productId);
        Task<LoanApplication> CreateAsync(LoanApplication loanApplication);
        Task<LoanApplication> UpdateAsync(LoanApplication loanApplication);
        Task<bool> DeleteAsync(Guid id);
    }
}
