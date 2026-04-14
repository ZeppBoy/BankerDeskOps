using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;

namespace BankerDeskOps.Application.Services
{
    /// <summary>
    /// Read service for loan contracts.
    /// Contract creation is handled exclusively by <see cref="LoanService.DisburseAsync"/>.
    /// </summary>
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;

        public ContractService(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository
                ?? throw new ArgumentNullException(nameof(contractRepository));
        }

        public async Task<IEnumerable<ContractDto>> GetAllAsync()
        {
            var contracts = await _contractRepository.GetAllAsync();
            return contracts.Select(MapToDto);
        }

        public async Task<ContractDto?> GetByIdAsync(Guid id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            return contract is null ? null : MapToDto(contract);
        }

        public async Task<ContractDto?> GetByLoanIdAsync(Guid loanId)
        {
            var contract = await _contractRepository.GetByLoanIdAsync(loanId);
            return contract is null ? null : MapToDto(contract);
        }

        private static ContractDto MapToDto(Contract c) => new()
        {
            Id             = c.Id,
            ContractNumber = c.ContractNumber,
            LoanId         = c.LoanId,
            CustomerName   = c.CustomerName,
            LoanAmount     = c.LoanAmount,
            InterestRate   = c.InterestRate,
            TermMonths     = c.TermMonths,
            DisbursedAt    = c.DisbursedAt,
            Status         = c.Status,
            CreatedAt      = c.CreatedAt,
            UpdatedAt      = c.UpdatedAt
        };
    }
}
