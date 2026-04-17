using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(Guid id);
        Task<UserDto> CreateAsync(CreateUserRequest request);
        Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request);
        Task<UserDto> ActivateAsync(Guid id);
        Task<UserDto> DeactivateAsync(Guid id);
        Task<bool> DeleteAsync(Guid id);
        Task<LoginResponse> AuthenticateAsync(LoginRequest request);
    }
}
