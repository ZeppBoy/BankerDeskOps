using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Application.Services
{
    /// <summary>
    /// Service implementation for loan business logic.
    /// </summary>
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;

        public LoanService(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository ?? throw new ArgumentNullException(nameof(loanRepository));
        }

        public async Task<IEnumerable<LoanDto>> GetAllAsync()
        {
            var loans = await _loanRepository.GetAllAsync();
            return loans.Select(MapToDto);
        }

        public async Task<LoanDto?> GetByIdAsync(Guid id)
        {
            var loan = await _loanRepository.GetByIdAsync(id);
            return loan is null ? null : MapToDto(loan);
        }

        public async Task<LoanDto> CreateAsync(CreateLoanRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            
            if (request.Amount <= 0)
                throw new ArgumentException("Loan amount must be greater than zero.", nameof(request.Amount));

            var loan = new Loan
            {
                Id = Guid.NewGuid(),
                CustomerName = request.CustomerName,
                Amount = request.Amount,
                InterestRate = request.InterestRate,
                TermMonths = request.TermMonths,
                Status = LoanStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdLoan = await _loanRepository.CreateAsync(loan);
            return MapToDto(createdLoan);
        }

        public async Task<LoanDto> ApproveAsync(Guid id)
        {
            var loan = await _loanRepository.GetByIdAsync(id);
            if (loan is null)
                throw new InvalidOperationException($"Loan with ID {id} not found.");

            loan.Status = LoanStatus.Approved;
            loan.UpdatedAt = DateTime.UtcNow;

            var updatedLoan = await _loanRepository.UpdateAsync(loan);
            return MapToDto(updatedLoan);
        }

        public async Task<LoanDto> RejectAsync(Guid id)
        {
            var loan = await _loanRepository.GetByIdAsync(id);
            if (loan is null)
                throw new InvalidOperationException($"Loan with ID {id} not found.");

            loan.Status = LoanStatus.Rejected;
            loan.UpdatedAt = DateTime.UtcNow;

            var updatedLoan = await _loanRepository.UpdateAsync(loan);
            return MapToDto(updatedLoan);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _loanRepository.DeleteAsync(id);
        }

        private static LoanDto MapToDto(Loan loan)
        {
            return new LoanDto
            {
                Id = loan.Id,
                CustomerName = loan.CustomerName,
                Amount = loan.Amount,
                InterestRate = loan.InterestRate,
                TermMonths = loan.TermMonths,
                Status = loan.Status,
                CreatedAt = loan.CreatedAt,
                UpdatedAt = loan.UpdatedAt
            };
        }
    }
}
