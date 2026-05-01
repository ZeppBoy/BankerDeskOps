using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Services
{
    public class FeeService : IFeeService
    {
        private readonly IFeeRepository _repository;

        public FeeService(IFeeRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<FeeDto>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(MapToDto);
        }

        public async Task<FeeDto?> GetByIdAsync(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            return item is null ? null : MapToDto(item);
        }

        public async Task<IEnumerable<FeeDto>> GetByProductIdAsync(Guid productId)
        {
            var items = await _repository.GetByProductIdAsync(productId);
            return items.Select(MapToDto);
        }

        public async Task<FeeDto> CreateAsync(CreateFeeRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Fee name is required.");

            var entity = new Fee
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                Name = request.Name,
                Amount = request.Amount
            };

            var created = await _repository.CreateAsync(entity);
            return MapToDto(created);
        }

        public async Task<FeeDto> UpdateAsync(UpdateFeeRequest request)
        {
            var entity = await _repository.GetByIdAsync(request.Id);
            if (entity is null) throw new InvalidOperationException($"Fee with ID {request.Id} not found.");

            entity.Name = request.Name;
            entity.Amount = request.Amount;

            var updated = await _repository.UpdateAsync(entity);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static FeeDto MapToDto(Fee entity) => new()
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            Name = entity.Name,
            Amount = entity.Amount
        };
    }
}
