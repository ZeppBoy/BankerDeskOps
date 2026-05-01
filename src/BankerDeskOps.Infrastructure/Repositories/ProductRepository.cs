using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankerDeskOps.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.AsNoTracking().ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Product?> GetByNameAsync(string name)
        {
            return await _context.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            if (product is null)
                throw new ArgumentNullException(nameof(product));

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            if (product is null)
                throw new ArgumentNullException(nameof(product));

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return false;

            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
