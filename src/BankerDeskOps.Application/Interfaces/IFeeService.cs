using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    public interface IFeeService
    {
        Task<IEnumerable<FeeDto>> GetAllAsync();
        Task<FeeDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<FeeDto>> GetByProductIdAsync(Guid productId);
        Task<FeeDto> CreateAsync(CreateFeeRequest request);
        Task<FeeDto> UpdateAsync(UpdateFeeRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
