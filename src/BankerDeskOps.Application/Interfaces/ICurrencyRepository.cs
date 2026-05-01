using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    public interface ICurrencyRepository
    {
        Task<IEnumerable<Currency>> GetAllAsync();
        Task<Currency?> GetByIdAsync(Guid id);
        Task<Currency?> GetByCodeAsync(string code);
        Task<Currency> CreateAsync(Currency currency);
        Task<Currency> UpdateAsync(Currency currency);
        Task<bool> DeleteAsync(Guid id);
    }
}
