using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Services
{
    public class LoanApplicationService : ILoanApplicationService
    {
        private readonly ILoanApplicationRepository _repository;

        public LoanApplicationService(ILoanApplicationRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<LoanApplicationDto>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(MapToDto);
        }

        public async Task<LoanApplicationDto?> GetByIdAsync(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            return item is null ? null : MapToDto(item);
        }

        public async Task<LoanApplicationDto?> GetByRequestIdAsync(string requestId)
        {
            var item = await _repository.GetByRequestIdAsync(requestId);
            return item is null ? null : MapToDto(item);
        }

        public async Task<LoanApplicationDto> CreateAsync(CreateLoanApplicationRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (request.Amount <= 0) throw new ArgumentException("Amount must be greater than zero.");
            if (request.TermMonths <= 0) throw new ArgumentException("Term months must be greater than zero.");

            var entity = new LoanApplication
            {
                Id = Guid.NewGuid(),
                RequestId = GenerateRequestId(),
                ProductId = request.ProductId,
                Amount = request.Amount,
                TermMonths = request.TermMonths,
                Status = LoanApplicationStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(entity);
            return MapToDto(created);
        }

        public async Task<LoanApplicationDto> UpdateStatusAsync(Guid id, string status, string? comment = null)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity is null) throw new InvalidOperationException($"Loan application with ID {id} not found.");

            entity.Status = (LoanApplicationStatus)Enum.Parse(typeof(LoanApplicationStatus), status);
            entity.Comment = comment;
            entity.UpdatedAt = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(entity);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static LoanApplicationDto MapToDto(LoanApplication entity) => new()
        {
            Id = entity.Id,
            RequestId = entity.RequestId,
            ProductId = entity.ProductId,
            Amount = entity.Amount,
            TermMonths = entity.TermMonths,
            Status = entity.Status.ToString(),
            Comment = entity.Comment,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };

        private static string GenerateRequestId() => $"LA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
    }
}
