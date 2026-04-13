using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Repositories
{
    public class BankClientRepository : IBankClientRepository
    {
        private readonly AppDbContext _context;

        public BankClientRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<BankClient>> GetAllAsync()
            => await _context.BankClients.AsNoTracking().ToListAsync();

        public async Task<BankClient?> GetByIdAsync(Guid id)
            => await _context.BankClients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

        public async Task<BankClient?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));
            return await _context.BankClients.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == email.ToLowerInvariant());
        }

        public async Task<BankClient?> GetByNationalIdAsync(string nationalId)
        {
            if (string.IsNullOrWhiteSpace(nationalId)) throw new ArgumentNullException(nameof(nationalId));
            return await _context.BankClients.AsNoTracking()
                .FirstOrDefaultAsync(x => x.NationalId == nationalId);
        }

        public async Task<BankClient> CreateAsync(BankClient client)
        {
            if (client is null) throw new ArgumentNullException(nameof(client));
            _context.BankClients.Add(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<BankClient> UpdateAsync(BankClient client)
        {
            if (client is null) throw new ArgumentNullException(nameof(client));
            _context.BankClients.Update(client);
            await _context.SaveChangesAsync();
            return client;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var client = await _context.BankClients.FirstOrDefaultAsync(x => x.Id == id);
            if (client is null) return false;
            _context.BankClients.Remove(client);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
