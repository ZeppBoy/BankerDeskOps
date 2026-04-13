using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    public interface IBankClientRepository
    {
        Task<IEnumerable<BankClient>> GetAllAsync();
        Task<BankClient?> GetByIdAsync(Guid id);
        Task<BankClient?> GetByEmailAsync(string email);
        Task<BankClient?> GetByNationalIdAsync(string nationalId);
        Task<BankClient> CreateAsync(BankClient client);
        Task<BankClient> UpdateAsync(BankClient client);
        Task<bool> DeleteAsync(Guid id);
    }
}
