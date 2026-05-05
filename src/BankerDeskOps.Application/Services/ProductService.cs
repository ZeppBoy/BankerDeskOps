using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(MapToDto);
        }

        public async Task<ProductDto?> GetByIdAsync(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            return item is null ? null : MapToDto(item);
        }

        public async Task<ProductDto> CreateAsync(CreateProductRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Product name is required.");

            var entity = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive,
                Term = request.Term,
                MinAmount = request.MinAmount,
                MaxAmount = request.MaxAmount,
                CurrencyId = request.CurrencyId
            };

            var created = await _repository.CreateAsync(entity);
            return MapToDto(created);
        }

        public async Task<ProductDto> UpdateAsync(UpdateProductRequest request)
        {
            var entity = await _repository.GetByIdAsync(request.Id);
            if (entity is null) throw new InvalidOperationException($"Product with ID {request.Id} not found.");

            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.IsActive = request.IsActive;
            entity.Term = request.Term;
            entity.MinAmount = request.MinAmount;
            entity.MaxAmount = request.MaxAmount;
            entity.CurrencyId = request.CurrencyId;

            var updated = await _repository.UpdateAsync(entity);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static ProductDto MapToDto(Product entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            Term = entity.Term,
            MinAmount = entity.MinAmount,
            MaxAmount = entity.MaxAmount,
            CurrencyId = entity.CurrencyId
        };
    }
}
