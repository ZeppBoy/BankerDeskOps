using BankerDeskOps.Application.DTOs;

namespace BankerDeskOps.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(Guid id);
        Task<ProductDto> CreateAsync(CreateProductRequest request);
        Task<ProductDto> UpdateAsync(UpdateProductRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
