using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Services
{
    public class RateService : IRateService
    {
        private readonly IRateRepository _repository;

        public RateService(IRateRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<RateDto>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(MapToDto);
        }

        public async Task<RateDto?> GetByIdAsync(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            return item is null ? null : MapToDto(item);
        }

        public async Task<IEnumerable<RateDto>> GetByProductIdAsync(Guid productId)
        {
            var items = await _repository.GetByProductIdAsync(productId);
            return items.Select(MapToDto);
        }

        public async Task<RateDto> CreateAsync(CreateRateRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            var entity = new Rate
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                MinAmount = request.MinAmount,
                MaxAmount = request.MaxAmount,
                MinTermMonths = request.MinTermMonths,
                MaxTermMonths = request.MaxTermMonths,
                RateValue = request.RateValue
            };

            var created = await _repository.CreateAsync(entity);
            return MapToDto(created);
        }

        public async Task<RateDto> UpdateAsync(UpdateRateRequest request)
        {
            var entity = await _repository.GetByIdAsync(request.Id);
            if (entity is null) throw new InvalidOperationException($"Rate with ID {request.Id} not found.");

            entity.MinAmount = request.MinAmount;
            entity.MaxAmount = request.MaxAmount;
            entity.MinTermMonths = request.MinTermMonths;
            entity.MaxTermMonths = request.MaxTermMonths;
            entity.RateValue = request.RateValue;

            var updated = await _repository.UpdateAsync(entity);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static RateDto MapToDto(Rate entity) => new()
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            MinAmount = entity.MinAmount,
            MaxAmount = entity.MaxAmount,
            MinTermMonths = entity.MinTermMonths,
            MaxTermMonths = entity.MaxTermMonths,
            RateValue = entity.RateValue
        };
    }
}
