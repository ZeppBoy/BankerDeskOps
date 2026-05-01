using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    public interface IRateRepository
    {
        Task<IEnumerable<Rate>> GetAllAsync();
        Task<Rate?> GetByIdAsync(Guid id);
        Task<IEnumerable<Rate>> GetByProductIdAsync(Guid productId);
        Task<Rate> CreateAsync(Rate rate);
        Task<Rate> UpdateAsync(Rate rate);
        Task<bool> DeleteAsync(Guid id);
    }
}
