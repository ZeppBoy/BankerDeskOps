using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for RetailAccount entity data access.
    /// </summary>
    public class RetailAccountRepository : IRetailAccountRepository
    {
        private readonly AppDbContext _context;

        public RetailAccountRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<RetailAccount>> GetAllAsync()
        {
            return await _context.RetailAccounts.AsNoTracking().ToListAsync();
        }

        public async Task<RetailAccount?> GetByIdAsync(Guid id)
        {
            return await _context.RetailAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<RetailAccount?> GetByAccountNumberAsync(string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(accountNumber))
                throw new ArgumentNullException(nameof(accountNumber));

            return await _context.RetailAccounts.AsNoTracking().FirstOrDefaultAsync(x => x.AccountNumber == accountNumber);
        }

        public async Task<RetailAccount> CreateAsync(RetailAccount account)
        {
            if (account is null)
                throw new ArgumentNullException(nameof(account));

            _context.RetailAccounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<RetailAccount> UpdateAsync(RetailAccount account)
        {
            if (account is null)
                throw new ArgumentNullException(nameof(account));

            _context.RetailAccounts.Update(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var account = await _context.RetailAccounts.FirstOrDefaultAsync(x => x.Id == id);
            if (account is null)
                return false;

            _context.RetailAccounts.Remove(account);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
