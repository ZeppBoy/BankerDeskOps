using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Application.Services;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Domain.Enums;
using NSubstitute;
using Xunit;

namespace BankerDeskOps.Application.Tests.Services
{
    public class LoanServiceTests
    {
        private readonly ILoanRepository _mockRepository;
        private readonly IContractRepository _mockContractRepository;
        private readonly LoanService _loanService;

        public LoanServiceTests()
        {
            _mockRepository         = Substitute.For<ILoanRepository>();
            _mockContractRepository = Substitute.For<IContractRepository>();
            _loanService            = new LoanService(_mockRepository, _mockContractRepository);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllLoans()
        {
            // Arrange
            var loans = new List<Loan>
            {
                new Loan { Id = Guid.NewGuid(), CustomerName = "John Doe", Amount = 10000m, InterestRate = 5.5m, TermMonths = 12, Status = LoanStatus.Pending },
                new Loan { Id = Guid.NewGuid(), CustomerName = "Jane Smith", Amount = 20000m, InterestRate = 4.5m, TermMonths = 24, Status = LoanStatus.Approved }
            };
            _mockRepository.GetAllAsync().Returns(loans);

            // Act
            var result = await _loanService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("John Doe", result.First().CustomerName);
            await _mockRepository.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnLoan()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            var loan = new Loan
            {
                Id = loanId,
                CustomerName = "John Doe",
                Amount = 10000m,
                InterestRate = 5.5m,
                TermMonths = 12,
                Status = LoanStatus.Pending
            };
            _mockRepository.GetByIdAsync(loanId).Returns(loan);

            // Act
            var result = await _loanService.GetByIdAsync(loanId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(loanId, result.Id);
            Assert.Equal("John Doe", result.CustomerName);
            await _mockRepository.Received(1).GetByIdAsync(loanId);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            _mockRepository.GetByIdAsync(loanId).Returns(default(Loan));

            // Act
            var result = await _loanService.GetByIdAsync(loanId);

            // Assert
            Assert.Null(result);
            await _mockRepository.Received(1).GetByIdAsync(loanId);
        }

        [Fact]
        public async Task CreateAsync_WithValidRequest_ShouldCreateLoan()
        {
            // Arrange
            var request = new CreateLoanRequest
            {
                CustomerName = "John Doe",
                Amount = 10000m,
                InterestRate = 5.5m,
                TermMonths = 12
            };
            
            // Configure mock to return a loan when CreateAsync is called
            _mockRepository.CreateAsync(Arg.Any<Loan>())
                .Returns(x => Task.FromResult(x.Arg<Loan>()));

            // Act
            var result = await _loanService.CreateAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Doe", result.CustomerName);
            Assert.Equal(10000m, result.Amount);
            Assert.Equal(LoanStatus.Pending, result.Status);
            await _mockRepository.Received(1).CreateAsync(Arg.Any<Loan>());
        }

        [Fact]
        public async Task CreateAsync_WithZeroAmount_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new CreateLoanRequest
            {
                CustomerName = "John Doe",
                Amount = 0m,
                InterestRate = 5.5m,
                TermMonths = 12
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _loanService.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_WithNegativeAmount_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new CreateLoanRequest
            {
                CustomerName = "John Doe",
                Amount = -1000m,
                InterestRate = 5.5m,
                TermMonths = 12
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _loanService.CreateAsync(request));
        }

        [Fact]
        public async Task ApproveAsync_WithPendingLoan_ShouldChangeStatusToApproved()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            var loan = new Loan
            {
                Id = loanId,
                CustomerName = "John Doe",
                Amount = 10000m,
                InterestRate = 5.5m,
                TermMonths = 12,
                Status = LoanStatus.Pending
            };
            _mockRepository.GetByIdAsync(loanId).Returns(loan);
            // Configure mock to return the updated loan when UpdateAsync is called
            _mockRepository.UpdateAsync(Arg.Any<Loan>())
                .Returns(x => Task.FromResult(x.Arg<Loan>()));

            // Act
            var result = await _loanService.ApproveAsync(loanId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(LoanStatus.Approved, result.Status);
            await _mockRepository.Received(1).GetByIdAsync(loanId);
            await _mockRepository.Received(1).UpdateAsync(Arg.Any<Loan>());
        }

        [Fact]
        public async Task ApproveAsync_WithNonexistentLoan_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            _mockRepository.GetByIdAsync(loanId).Returns(default(Loan));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _loanService.ApproveAsync(loanId));
        }

        [Fact]
        public async Task RejectAsync_WithPendingLoan_ShouldChangeStatusToRejected()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            var loan = new Loan
            {
                Id = loanId,
                CustomerName = "John Doe",
                Amount = 10000m,
                InterestRate = 5.5m,
                TermMonths = 12,
                Status = LoanStatus.Pending
            };
            _mockRepository.GetByIdAsync(loanId).Returns(loan);
            // Configure mock to return the updated loan when UpdateAsync is called
            _mockRepository.UpdateAsync(Arg.Any<Loan>())
                .Returns(x => Task.FromResult(x.Arg<Loan>()));

            // Act
            var result = await _loanService.RejectAsync(loanId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(LoanStatus.Rejected, result.Status);
            await _mockRepository.Received(1).GetByIdAsync(loanId);
            await _mockRepository.Received(1).UpdateAsync(Arg.Any<Loan>());
        }

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldDeleteLoan()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            var loan = new Loan
            {
                Id = loanId,
                CustomerName = "John Doe",
                Amount = 10000m,
                InterestRate = 5.5m,
                TermMonths = 12,
                Status = LoanStatus.Pending
            };
            _mockRepository.GetByIdAsync(loanId).Returns(loan);
            // Configure mock to return true for DeleteAsync
            _mockRepository.DeleteAsync(loanId).Returns(true);

            // Act
            var result = await _loanService.DeleteAsync(loanId);

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).DeleteAsync(loanId);
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            _mockRepository.GetByIdAsync(loanId).Returns(default(Loan));
            // Configure mock to return false for DeleteAsync (loan doesn't exist)
            _mockRepository.DeleteAsync(loanId).Returns(false);

            // Act
            var result = await _loanService.DeleteAsync(loanId);

            // Assert
            Assert.False(result);
            await _mockRepository.Received(1).DeleteAsync(loanId);
        }

        // ── DisburseAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task DisburseAsync_WithApprovedLoan_ShouldSetDisbursedAndCreateContract()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            var loan = new Loan
            {
                Id           = loanId,
                CustomerName = "Jane Disbursed",
                Amount       = 50000m,
                InterestRate = 6.5m,
                TermMonths   = 36,
                Status       = LoanStatus.Approved
            };
            _mockRepository.GetByIdAsync(loanId).Returns(loan);
            _mockRepository.UpdateAsync(Arg.Any<Loan>()).Returns(x => Task.FromResult(x.Arg<Loan>()));
            _mockContractRepository.CreateAsync(Arg.Any<Contract>())
                .Returns(x => Task.FromResult(x.Arg<Contract>()));

            // Act
            var result = await _loanService.DisburseAsync(loanId);

            // Assert
            Assert.Equal(LoanStatus.Disbursed, result.Status);
            await _mockRepository.Received(1).UpdateAsync(
                Arg.Is<Loan>(l => l.Status == LoanStatus.Disbursed));
            await _mockContractRepository.Received(1).CreateAsync(
                Arg.Is<Contract>(c =>
                    c.LoanId       == loanId &&
                    c.CustomerName == "Jane Disbursed" &&
                    c.LoanAmount   == 50000m &&
                    c.Status       == ContractStatus.Active &&
                    c.ContractNumber.StartsWith("CNT-")));
        }

        [Fact]
        public async Task DisburseAsync_WithPendingLoan_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            var loan = new Loan { Id = loanId, Status = LoanStatus.Pending };
            _mockRepository.GetByIdAsync(loanId).Returns(loan);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _loanService.DisburseAsync(loanId));

            Assert.Contains("Approved", ex.Message);
            await _mockContractRepository.DidNotReceive().CreateAsync(Arg.Any<Contract>());
        }

        [Fact]
        public async Task DisburseAsync_WithAlreadyDisbursedLoan_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var loanId = Guid.NewGuid();
            var loan = new Loan { Id = loanId, Status = LoanStatus.Disbursed };
            _mockRepository.GetByIdAsync(loanId).Returns(loan);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _loanService.DisburseAsync(loanId));
            await _mockContractRepository.DidNotReceive().CreateAsync(Arg.Any<Contract>());
        }

        [Fact]
        public async Task DisburseAsync_WithNonExistentLoan_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _mockRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(default(Loan));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _loanService.DisburseAsync(Guid.NewGuid()));
        }
    }
}
