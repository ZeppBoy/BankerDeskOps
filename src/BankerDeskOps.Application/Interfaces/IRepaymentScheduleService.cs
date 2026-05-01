using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    public interface IRepaymentScheduleService
    {
        Task<IEnumerable<RepaymentScheduleDto>> GetAllAsync();
        Task<RepaymentScheduleDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<RepaymentScheduleDto>> GetByLoanApplicationIdAsync(Guid loanApplicationId);
        Task<RepaymentScheduleDto> CreateAsync(CreateRepaymentScheduleRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
