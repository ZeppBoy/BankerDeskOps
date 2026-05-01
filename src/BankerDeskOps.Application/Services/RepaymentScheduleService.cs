using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Services
{
    public class RepaymentScheduleService : IRepaymentScheduleService
    {
        private readonly IRepaymentScheduleRepository _repository;

        public RepaymentScheduleService(IRepaymentScheduleRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<IEnumerable<RepaymentScheduleDto>> GetAllAsync()
        {
            var items = await _repository.GetAllAsync();
            return items.Select(MapToDto);
        }

        public async Task<RepaymentScheduleDto?> GetByIdAsync(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            return item is null ? null : MapToDto(item);
        }

        public async Task<IEnumerable<RepaymentScheduleDto>> GetByLoanApplicationIdAsync(Guid loanApplicationId)
        {
            var items = await _repository.GetByLoanApplicationIdAsync(loanApplicationId);
            return items.Select(MapToDto);
        }

        public async Task<RepaymentScheduleDto> CreateAsync(CreateRepaymentScheduleRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            var entity = new RepaymentSchedule
            {
                ScheduleId = Guid.NewGuid(),
                LoanApplicationId = request.LoanApplicationId,
                PaymentNumber = request.PaymentNumber,
                DueDate = request.DueDate,
                PrincipalAmount = request.PrincipalAmount,
                InterestAmount = request.InterestAmount,
                TotalPayment = request.TotalPayment
            };

            var created = await _repository.CreateAsync(entity);
            return MapToDto(created);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repository.DeleteAsync(id);
        }

        private static RepaymentScheduleDto MapToDto(RepaymentSchedule entity) => new()
        {
            ScheduleId = entity.ScheduleId,
            LoanApplicationId = entity.LoanApplicationId,
            PaymentNumber = entity.PaymentNumber,
            DueDate = entity.DueDate,
            PrincipalAmount = entity.PrincipalAmount,
            InterestAmount = entity.InterestAmount,
            TotalPayment = entity.TotalPayment
        };
    }
}
