using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Repositories
{
    public class FeeRepository : IFeeRepository
    {
        private readonly AppDbContext _context;

        public FeeRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Fee>> GetAllAsync()
        {
            return await _context.Fees.AsNoTracking().ToListAsync();
        }

        public async Task<Fee?> GetByIdAsync(Guid id)
        {
            return await _context.Fees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Fee>> GetByProductIdAsync(Guid productId)
        {
            return await _context.Fees.AsNoTracking().Where(x => x.ProductId == productId).ToListAsync();
        }

        public async Task<Fee> CreateAsync(Fee fee)
        {
            if (fee is null)
                throw new ArgumentNullException(nameof(fee));

            _context.Fees.Add(fee);
            await _context.SaveChangesAsync();
            return fee;
        }

        public async Task<Fee> UpdateAsync(Fee fee)
        {
            if (fee is null)
                throw new ArgumentNullException(nameof(fee));

            _context.Fees.Update(fee);
            await _context.SaveChangesAsync();
            return fee;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Fees.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return false;

            _context.Fees.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
