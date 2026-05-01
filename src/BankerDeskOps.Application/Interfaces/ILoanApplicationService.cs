using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    public interface ILoanApplicationService
    {
        Task<IEnumerable<LoanApplicationDto>> GetAllAsync();
        Task<LoanApplicationDto?> GetByIdAsync(Guid id);
        Task<LoanApplicationDto?> GetByRequestIdAsync(string requestId);
        Task<LoanApplicationDto> CreateAsync(CreateLoanApplicationRequest request);
        Task<LoanApplicationDto> UpdateStatusAsync(Guid id, string status, string? comment = null);
        Task<bool> DeleteAsync(Guid id);
    }
}
