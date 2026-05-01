using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Services
{
    public class CommissionService : ICommissionService
    {
        private readonly ICommissionRepository _repository;

        public CommissionService(ICommissionRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<CommissionDto>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(MapToDto);
        }

        public async Task<CommissionDto?> GetByIdAsync(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            return item is null ? null : MapToDto(item);
        }

        public async Task<IEnumerable<CommissionDto>> GetByProductIdAsync(Guid productId)
        {
            var items = await _repository.GetByProductIdAsync(productId);
            return items.Select(MapToDto);
        }

        public async Task<CommissionDto> CreateAsync(CreateCommissionRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Commission name is required.");

            var entity = new Commission
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                Name = request.Name,
                Percentage = request.Percentage
            };

            var created = await _repository.CreateAsync(entity);
            return MapToDto(created);
        }

        public async Task<CommissionDto> UpdateAsync(UpdateCommissionRequest request)
        {
            var entity = await _repository.GetByIdAsync(request.Id);
            if (entity is null) throw new InvalidOperationException($"Commission with ID {request.Id} not found.");

            entity.Name = request.Name;
            entity.Percentage = request.Percentage;

            var updated = await _repository.UpdateAsync(entity);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static CommissionDto MapToDto(Commission entity) => new()
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            Name = entity.Name,
            Percentage = entity.Percentage
        };
    }
}
