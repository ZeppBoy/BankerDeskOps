using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    public interface ICurrencyService
    {
        Task<IEnumerable<CurrencyDto>> GetAllAsync();
        Task<CurrencyDto?> GetByIdAsync(Guid id);
        Task<CurrencyDto?> GetByCodeAsync(string code);
        Task<CurrencyDto> CreateAsync(CreateCurrencyRequest request);
        Task<CurrencyDto> UpdateAsync(UpdateCurrencyRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
