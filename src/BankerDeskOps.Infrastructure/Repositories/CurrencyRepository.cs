using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Repositories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private readonly AppDbContext _context;

        public CurrencyRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Currency>> GetAllAsync()
        {
            return await _context.Currencies.AsNoTracking().ToListAsync();
        }

        public async Task<Currency?> GetByIdAsync(Guid id)
        {
            return await _context.Currencies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Currency?> GetByCodeAsync(string code)
        {
            return await _context.Currencies.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code);
        }

        public async Task<Currency> CreateAsync(Currency currency)
        {
            if (currency is null)
                throw new ArgumentNullException(nameof(currency));

            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();
            return currency;
        }

        public async Task<Currency> UpdateAsync(Currency currency)
        {
            if (currency is null)
                throw new ArgumentNullException(nameof(currency));

            _context.Currencies.Update(currency);
            await _context.SaveChangesAsync();
            return currency;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Currencies.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return false;

            _context.Currencies.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
