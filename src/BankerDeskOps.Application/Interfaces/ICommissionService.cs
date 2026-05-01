using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    public interface ICommissionService
    {
        Task<IEnumerable<CommissionDto>> GetAllAsync();
        Task<CommissionDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<CommissionDto>> GetByProductIdAsync(Guid productId);
        Task<CommissionDto> CreateAsync(CreateCommissionRequest request);
        Task<CommissionDto> UpdateAsync(UpdateCommissionRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
