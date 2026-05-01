using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Repositories
{
    public class RateRepository : IRateRepository
    {
        private readonly AppDbContext _context;

        public RateRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Rate>> GetAllAsync()
        {
            return await _context.Rates.AsNoTracking().ToListAsync();
        }

        public async Task<Rate?> GetByIdAsync(Guid id)
        {
            return await _context.Rates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Rate>> GetByProductIdAsync(Guid productId)
        {
            return await _context.Rates.AsNoTracking().Where(x => x.ProductId == productId).ToListAsync();
        }

        public async Task<Rate> CreateAsync(Rate rate)
        {
            if (rate is null)
                throw new ArgumentNullException(nameof(rate));

            _context.Rates.Add(rate);
            await _context.SaveChangesAsync();
            return rate;
        }

        public async Task<Rate> UpdateAsync(Rate rate)
        {
            if (rate is null)
                throw new ArgumentNullException(nameof(rate));

            _context.Rates.Update(rate);
            await _context.SaveChangesAsync();
            return rate;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Rates.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return false;

            _context.Rates.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
