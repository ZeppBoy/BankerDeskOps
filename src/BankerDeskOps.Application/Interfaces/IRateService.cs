using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    public interface IRateService
    {
        Task<IEnumerable<RateDto>> GetAllAsync();
        Task<RateDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<RateDto>> GetByProductIdAsync(Guid productId);
        Task<RateDto> CreateAsync(CreateRateRequest request);
        Task<RateDto> UpdateAsync(UpdateRateRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
