using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Repositories
{
    public class LoanApplicationRepository : ILoanApplicationRepository
    {
        private readonly AppDbContext _context;

        public LoanApplicationRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<LoanApplication>> GetAllAsync()
        {
            return await _context.LoanApplications.AsNoTracking().ToListAsync();
        }

        public async Task<LoanApplication?> GetByIdAsync(Guid id)
        {
            return await _context.LoanApplications.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<LoanApplication?> GetByRequestIdAsync(string requestId)
        {
            return await _context.LoanApplications.AsNoTracking().FirstOrDefaultAsync(x => x.RequestId == requestId);
        }

        public async Task<IEnumerable<LoanApplication>> GetByProductIdAsync(Guid productId)
        {
            return await _context.LoanApplications.AsNoTracking().Where(x => x.ProductId == productId).ToListAsync();
        }

        public async Task<LoanApplication> CreateAsync(LoanApplication loanApplication)
        {
            if (loanApplication is null)
                throw new ArgumentNullException(nameof(loanApplication));

            _context.LoanApplications.Add(loanApplication);
            await _context.SaveChangesAsync();
            return loanApplication;
        }

        public async Task<LoanApplication> UpdateAsync(LoanApplication loanApplication)
        {
            if (loanApplication is null)
                throw new ArgumentNullException(nameof(loanApplication));

            _context.LoanApplications.Update(loanApplication);
            await _context.SaveChangesAsync();
            return loanApplication;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.LoanApplications.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return false;

            _context.LoanApplications.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
