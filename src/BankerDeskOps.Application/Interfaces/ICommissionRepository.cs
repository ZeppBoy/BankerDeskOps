using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Interfaces
{
    public interface ICommissionRepository
    {
        Task<IEnumerable<Commission>> GetAllAsync();
        Task<Commission?> GetByIdAsync(Guid id);
        Task<IEnumerable<Commission>> GetByProductIdAsync(Guid productId);
        Task<Commission> CreateAsync(Commission commission);
        Task<Commission> UpdateAsync(Commission commission);
        Task<bool> DeleteAsync(Guid id);
    }
}
