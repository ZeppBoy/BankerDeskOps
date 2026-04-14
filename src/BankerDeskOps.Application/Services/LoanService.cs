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
        private readonly IContractRepository _contractRepository;

        public LoanService(ILoanRepository loanRepository, IContractRepository contractRepository)
        {
            _loanRepository     = loanRepository     ?? throw new ArgumentNullException(nameof(loanRepository));
            _contractRepository = contractRepository  ?? throw new ArgumentNullException(nameof(contractRepository));
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

            if (loan.Status != LoanStatus.Pending)
                throw new InvalidOperationException(
                    $"Only a Pending loan can be approved. Current status: {loan.Status}.");

            loan.Status    = LoanStatus.Approved;
            loan.UpdatedAt = DateTime.UtcNow;

            var updatedLoan = await _loanRepository.UpdateAsync(loan);
            return MapToDto(updatedLoan);
        }

        public async Task<LoanDto> RejectAsync(Guid id)
        {
            var loan = await _loanRepository.GetByIdAsync(id);
            if (loan is null)
                throw new InvalidOperationException($"Loan with ID {id} not found.");

            loan.Status    = LoanStatus.Rejected;
            loan.UpdatedAt = DateTime.UtcNow;

            var updatedLoan = await _loanRepository.UpdateAsync(loan);
            return MapToDto(updatedLoan);
        }

        /// <inheritdoc />
        public async Task<LoanDto> DisburseAsync(Guid id)
        {
            var loan = await _loanRepository.GetByIdAsync(id);
            if (loan is null)
                throw new InvalidOperationException($"Loan with ID {id} not found.");

            if (loan.Status != LoanStatus.Approved)
                throw new InvalidOperationException(
                    $"Only an Approved loan can be disbursed. Current status: {loan.Status}.");

            var disbursedAt = DateTime.UtcNow;

            // 1. Transition loan status
            loan.Status    = LoanStatus.Disbursed;
            loan.UpdatedAt = disbursedAt;
            var updatedLoan = await _loanRepository.UpdateAsync(loan);

            // 2. Create contract — single entry point enforced in the service layer
            var contract = new Contract
            {
                Id             = Guid.NewGuid(),
                ContractNumber = GenerateContractNumber(),
                LoanId         = loan.Id,
                CustomerName   = loan.CustomerName,
                LoanAmount     = loan.Amount,
                InterestRate   = loan.InterestRate,
                TermMonths     = loan.TermMonths,
                DisbursedAt    = disbursedAt,
                Status         = ContractStatus.Active,
                CreatedAt      = disbursedAt,
                UpdatedAt      = disbursedAt
            };
            await _contractRepository.CreateAsync(contract);

            return MapToDto(updatedLoan);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _loanRepository.DeleteAsync(id);
        }

        // ── helpers ─────────────────────────────────────────────────────────────────

        private static LoanDto MapToDto(Loan loan) => new()
        {
            Id           = loan.Id,
            CustomerName = loan.CustomerName,
            Amount       = loan.Amount,
            InterestRate = loan.InterestRate,
            TermMonths   = loan.TermMonths,
            Status       = loan.Status,
            CreatedAt    = loan.CreatedAt,
            UpdatedAt    = loan.UpdatedAt
        };

        /// <summary>
        /// Generates a human-readable, unique contract number: CNT-{YYYY}-{8 uppercase hex chars}.
        /// </summary>
        private static string GenerateContractNumber()
            => $"CNT-{DateTime.UtcNow:yyyy}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
}
