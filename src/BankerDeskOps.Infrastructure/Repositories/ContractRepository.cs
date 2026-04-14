using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for Contract entity data access.
    /// </summary>
    public class ContractRepository : IContractRepository
    {
        private readonly AppDbContext _context;

        public ContractRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Contract>> GetAllAsync()
            => await _context.Contracts.AsNoTracking().ToListAsync();

        public async Task<Contract?> GetByIdAsync(Guid id)
            => await _context.Contracts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

        public async Task<Contract?> GetByLoanIdAsync(Guid loanId)
            => await _context.Contracts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.LoanId == loanId);

        public async Task<Contract?> GetByContractNumberAsync(string contractNumber)
            => await _context.Contracts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ContractNumber == contractNumber);

        public async Task<Contract> CreateAsync(Contract contract)
        {
            if (contract is null) throw new ArgumentNullException(nameof(contract));
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<Contract> UpdateAsync(Contract contract)
        {
            if (contract is null) throw new ArgumentNullException(nameof(contract));
            _context.Contracts.Update(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var contract = await _context.Contracts.FirstOrDefaultAsync(x => x.Id == id);
            if (contract is null) return false;
            _context.Contracts.Remove(contract);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
