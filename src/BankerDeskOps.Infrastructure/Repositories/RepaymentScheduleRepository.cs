using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Repositories
{
    public class RepaymentScheduleRepository : IRepaymentScheduleRepository
    {
        private readonly AppDbContext _context;

        public RepaymentScheduleRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<RepaymentSchedule>> GetAllAsync()
        {
            return await _context.RepaymentSchedules.AsNoTracking().ToListAsync();
        }

        public async Task<RepaymentSchedule?> GetByIdAsync(Guid id)
        {
            return await _context.RepaymentSchedules.AsNoTracking().FirstOrDefaultAsync(x => x.ScheduleId == id);
        }

        public async Task<IEnumerable<RepaymentSchedule>> GetByLoanApplicationIdAsync(Guid loanApplicationId)
        {
            return await _context.RepaymentSchedules.AsNoTracking()
                .Where(x => x.LoanApplicationId == loanApplicationId)
                .ToListAsync();
        }

        public async Task<RepaymentSchedule> CreateAsync(RepaymentSchedule schedule)
        {
            if (schedule is null)
                throw new ArgumentNullException(nameof(schedule));

            _context.RepaymentSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<RepaymentSchedule> UpdateAsync(RepaymentSchedule schedule)
        {
            if (schedule is null)
                throw new ArgumentNullException(nameof(schedule));

            _context.RepaymentSchedules.Update(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.RepaymentSchedules.FirstOrDefaultAsync(x => x.ScheduleId == id);
            if (entity is null)
                return false;

            _context.RepaymentSchedules.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
