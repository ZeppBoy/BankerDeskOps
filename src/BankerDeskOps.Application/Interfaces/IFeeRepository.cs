using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    public interface IFeeRepository
    {
        Task<IEnumerable<Fee>> GetAllAsync();
        Task<Fee?> GetByIdAsync(Guid id);
        Task<IEnumerable<Fee>> GetByProductIdAsync(Guid productId);
        Task<Fee> CreateAsync(Fee fee);
        Task<Fee> UpdateAsync(Fee fee);
        Task<bool> DeleteAsync(Guid id);
    }
}
