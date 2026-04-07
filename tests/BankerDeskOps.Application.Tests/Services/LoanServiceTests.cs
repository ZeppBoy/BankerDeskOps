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
        private readonly LoanService _loanService;

        public LoanServiceTests()
        {
            _mockRepository = Substitute.For<ILoanRepository>();
            _loanService = new LoanService(_mockRepository);
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
            _mockRepository.GetByIdAsync(loanId).Returns((Loan)null);

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
    }
}
