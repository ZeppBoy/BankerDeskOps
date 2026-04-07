using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Loan entity data access.
    /// </summary>
    public class LoanRepository : ILoanRepository
    {
        private readonly AppDbContext _context;

        public LoanRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Loan>> GetAllAsync()
        {
            return await _context.Loans.AsNoTracking().ToListAsync();
        }

        public async Task<Loan?> GetByIdAsync(Guid id)
        {
            return await _context.Loans.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Loan> CreateAsync(Loan loan)
        {
            if (loan is null)
                throw new ArgumentNullException(nameof(loan));

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();
            return loan;
        }

        public async Task<Loan> UpdateAsync(Loan loan)
        {
            if (loan is null)
                throw new ArgumentNullException(nameof(loan));

            _context.Loans.Update(loan);
            await _context.SaveChangesAsync();
            return loan;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var loan = await _context.Loans.FirstOrDefaultAsync(x => x.Id == id);
            if (loan is null)
                return false;

            _context.Loans.Remove(loan);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
