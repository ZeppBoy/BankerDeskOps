using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Repositories
{
    public class CommissionRepository : ICommissionRepository
    {
        private readonly AppDbContext _context;

        public CommissionRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Commission>> GetAllAsync()
        {
            return await _context.Commissions.AsNoTracking().ToListAsync();
        }

        public async Task<Commission?> GetByIdAsync(Guid id)
        {
            return await _context.Commissions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Commission>> GetByProductIdAsync(Guid productId)
        {
            return await _context.Commissions.AsNoTracking().Where(x => x.ProductId == productId).ToListAsync();
        }

        public async Task<Commission> CreateAsync(Commission commission)
        {
            if (commission is null)
                throw new ArgumentNullException(nameof(commission));

            _context.Commissions.Add(commission);
            await _context.SaveChangesAsync();
            return commission;
        }

        public async Task<Commission> UpdateAsync(Commission commission)
        {
            if (commission is null)
                throw new ArgumentNullException(nameof(commission));

            _context.Commissions.Update(commission);
            await _context.SaveChangesAsync();
            return commission;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Commissions.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return false;

            _context.Commissions.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
