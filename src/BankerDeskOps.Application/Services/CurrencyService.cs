using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ICurrencyRepository _repository;

        public CurrencyService(ICurrencyRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<CurrencyDto>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(MapToDto);
        }

        public async Task<CurrencyDto?> GetByIdAsync(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            return item is null ? null : MapToDto(item);
        }

        public async Task<CurrencyDto?> GetByCodeAsync(string code)
        {
            var item = await _repository.GetByCodeAsync(code);
            return item is null ? null : MapToDto(item);
        }

        public async Task<CurrencyDto> CreateAsync(CreateCurrencyRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(request.Code)) throw new ArgumentException("Currency code is required.");
            if (string.IsNullOrWhiteSpace(request.Name)) throw new ArgumentException("Currency name is required.");

            var entity = new Currency
            {
                Id = Guid.NewGuid(),
                Code = request.Code,
                Name = request.Name
            };

            var created = await _repository.CreateAsync(entity);
            return MapToDto(created);
        }

        public async Task<CurrencyDto> UpdateAsync(UpdateCurrencyRequest request)
        {
            var entity = await _repository.GetByIdAsync(request.Id);
            if (entity is null) throw new InvalidOperationException($"Currency with ID {request.Id} not found.");

            entity.Code = request.Code;
            entity.Name = request.Name;

            var updated = await _repository.UpdateAsync(entity);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static CurrencyDto MapToDto(Currency entity) => new()
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name
        };
    }
}
