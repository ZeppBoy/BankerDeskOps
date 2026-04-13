using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    public interface IBankClientService
    {
        Task<IEnumerable<BankClientDto>> GetAllAsync();
        Task<BankClientDto?> GetByIdAsync(Guid id);
        Task<BankClientDto> CreateAsync(CreateBankClientRequest request);
        Task<BankClientDto> UpdateAsync(Guid id, UpdateBankClientRequest request);
        Task<BankClientDto> SuspendAsync(Guid id);
        Task<BankClientDto> ActivateAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
    }
}
