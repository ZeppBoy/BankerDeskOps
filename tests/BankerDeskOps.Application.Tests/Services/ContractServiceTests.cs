using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Application.Services;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Domain.Enums;
using NSubstitute;
using Xunit;

namespace BankerDeskOps.Application.Tests.Services
{
    public class ContractServiceTests
    {
        private readonly IContractRepository _mockRepository;
        private readonly ContractService _contractService;

        public ContractServiceTests()
        {
            _mockRepository  = Substitute.For<IContractRepository>();
            _contractService = new ContractService(_mockRepository);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllContracts()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            var contracts = new List<Contract>
            {
                new()
                {
                    Id             = Guid.NewGuid(),
                    ContractNumber = "CNT-2026-AAAABBBB",
                    LoanId         = loanId,
                    CustomerName   = "Alice Smith",
                    LoanAmount     = 25000m,
                    InterestRate   = 5.0m,
                    TermMonths     = 24,
                    DisbursedAt    = DateTime.UtcNow,
                    Status         = ContractStatus.Active
                }
            };
            _mockRepository.GetAllAsync().Returns(contracts);

            // Act
            var result = await _contractService.GetAllAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("CNT-2026-AAAABBBB", result.First().ContractNumber);
            await _mockRepository.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnContractDto()
        {
            // Arrange
            var contractId = Guid.NewGuid();
            var contract = new Contract
            {
                Id             = contractId,
                ContractNumber = "CNT-2026-CCCCDDDD",
                LoanId         = Guid.NewGuid(),
                CustomerName   = "Bob Jones",
                LoanAmount     = 10000m,
                InterestRate   = 7.5m,
                TermMonths     = 12,
                DisbursedAt    = DateTime.UtcNow,
                Status         = ContractStatus.Active
            };
            _mockRepository.GetByIdAsync(contractId).Returns(contract);

            // Act
            var result = await _contractService.GetByIdAsync(contractId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(contractId, result!.Id);
            Assert.Equal("CNT-2026-CCCCDDDD", result.ContractNumber);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            _mockRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(default(Contract));

            // Act
            var result = await _contractService.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByLoanIdAsync_WithMatchingLoan_ShouldReturnContractDto()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            var contract = new Contract
            {
                Id             = Guid.NewGuid(),
                ContractNumber = "CNT-2026-EEEEFFFF",
                LoanId         = loanId,
                CustomerName   = "Carol White",
                LoanAmount     = 30000m,
                InterestRate   = 4.25m,
                TermMonths     = 48,
                DisbursedAt    = DateTime.UtcNow,
                Status         = ContractStatus.Active
            };
            _mockRepository.GetByLoanIdAsync(loanId).Returns(contract);

            // Act
            var result = await _contractService.GetByLoanIdAsync(loanId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(loanId, result!.LoanId);
            Assert.Equal(ContractStatus.Active, result.Status);
        }

        [Fact]
        public async Task GetByLoanIdAsync_WithNoContract_ShouldReturnNull()
        {
            // Arrange
            _mockRepository.GetByLoanIdAsync(Arg.Any<Guid>()).Returns(default(Contract));

            // Act
            var result = await _contractService.GetByLoanIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }
    }
}
