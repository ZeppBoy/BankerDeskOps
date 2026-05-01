using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    public interface IRepaymentScheduleRepository
    {
        Task<IEnumerable<RepaymentSchedule>> GetAllAsync();
        Task<RepaymentSchedule?> GetByIdAsync(Guid id);
        Task<IEnumerable<RepaymentSchedule>> GetByLoanApplicationIdAsync(Guid loanApplicationId);
        Task<RepaymentSchedule> CreateAsync(RepaymentSchedule schedule);
        Task<RepaymentSchedule> UpdateAsync(RepaymentSchedule schedule);
        Task<bool> DeleteAsync(Guid id);
    }
}
