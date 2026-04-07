using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Application.Services;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Domain.Enums;
using NSubstitute;
using Xunit;

namespace BankerDeskOps.Application.Tests.Services
{
    public class RetailAccountServiceTests
    {
        private readonly IRetailAccountRepository _mockRepository;
        private readonly RetailAccountService _accountService;

        public RetailAccountServiceTests()
        {
            _mockRepository = Substitute.For<IRetailAccountRepository>();
            _accountService = new RetailAccountService(_mockRepository);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllAccounts()
        {
            // Arrange
            var accounts = new List<RetailAccount>
            {
                new RetailAccount { Id = Guid.NewGuid(), CustomerName = "John Doe", AccountNumber = "ACC001", Balance = 5000m, AccountType = AccountType.Checking },
                new RetailAccount { Id = Guid.NewGuid(), CustomerName = "Jane Smith", AccountNumber = "ACC002", Balance = 10000m, AccountType = AccountType.Savings }
            };
            _mockRepository.GetAllAsync().Returns(accounts);

            // Act
            var result = await _accountService.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("John Doe", result.First().CustomerName);
            await _mockRepository.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnAccount()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new RetailAccount
            {
                Id = accountId,
                CustomerName = "John Doe",
                AccountNumber = "ACC001",
                Balance = 5000m,
                AccountType = AccountType.Checking
            };
            _mockRepository.GetByIdAsync(accountId).Returns(account);

            // Act
            var result = await _accountService.GetByIdAsync(accountId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(accountId, result.Id);
            Assert.Equal("John Doe", result.CustomerName);
            await _mockRepository.Received(1).GetByIdAsync(accountId);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            _mockRepository.GetByIdAsync(accountId).Returns(default(RetailAccount));

            // Act
            var result = await _accountService.GetByIdAsync(accountId);

            // Assert
            Assert.Null(result);
            await _mockRepository.Received(1).GetByIdAsync(accountId);
        }

        [Fact]
        public async Task OpenAsync_WithValidRequest_ShouldOpenAccount()
        {
            // Arrange
            var request = new CreateRetailAccountRequest
            {
                CustomerName = "John Doe",
                AccountType = AccountType.Checking,
                InitialDeposit = 0m
            };
            // Configure mock to return the account that was passed to CreateAsync
            _mockRepository.CreateAsync(Arg.Any<RetailAccount>())
                .Returns(x => Task.FromResult(x.Arg<RetailAccount>()));

            // Act
            var result = await _accountService.OpenAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John Doe", result.CustomerName);
            Assert.Equal(0m, result.Balance);
            Assert.Equal(AccountType.Checking, result.AccountType);
            Assert.NotEmpty(result.AccountNumber);
            await _mockRepository.Received(1).CreateAsync(Arg.Any<RetailAccount>());
        }

        [Fact]
        public async Task OpenAsync_WithEmptyCustomerName_ShouldThrowArgumentException()
        {
            // Arrange
            var request = new CreateRetailAccountRequest
            {
                CustomerName = "",
                AccountType = AccountType.Checking,
                InitialDeposit = 0m
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _accountService.OpenAsync(request));
        }

        [Fact]
        public async Task DepositAsync_WithValidAmount_ShouldIncreaseBalance()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new RetailAccount
            {
                Id = accountId,
                CustomerName = "John Doe",
                AccountNumber = "ACC001",
                Balance = 5000m,
                AccountType = AccountType.Checking
            };
            var request = new DepositRequest { Amount = 1000m };
            _mockRepository.GetByIdAsync(accountId).Returns(account);
            // Configure mock to return the updated account when UpdateAsync is called
            _mockRepository.UpdateAsync(Arg.Any<RetailAccount>())
                .Returns(x => Task.FromResult(x.Arg<RetailAccount>()));

            // Act
            var result = await _accountService.DepositAsync(accountId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(6000m, result.Balance);
            await _mockRepository.Received(1).GetByIdAsync(accountId);
            await _mockRepository.Received(1).UpdateAsync(Arg.Any<RetailAccount>());
        }

        [Fact]
        public async Task DepositAsync_WithZeroAmount_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new DepositRequest { Amount = 0m };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _accountService.DepositAsync(accountId, request));
        }

        [Fact]
        public async Task DepositAsync_WithNegativeAmount_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new DepositRequest { Amount = -500m };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _accountService.DepositAsync(accountId, request));
        }

        [Fact]
        public async Task DepositAsync_WithNonexistentAccount_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new DepositRequest { Amount = 1000m };
            _mockRepository.GetByIdAsync(accountId).Returns(default(RetailAccount));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _accountService.DepositAsync(accountId, request));
        }

        [Fact]
        public async Task WithdrawAsync_WithValidAmount_ShouldDecreaseBalance()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new RetailAccount
            {
                Id = accountId,
                CustomerName = "John Doe",
                AccountNumber = "ACC001",
                Balance = 5000m,
                AccountType = AccountType.Checking
            };
            var request = new WithdrawRequest { Amount = 1000m };
            _mockRepository.GetByIdAsync(accountId).Returns(account);
            // Configure mock to return the updated account when UpdateAsync is called
            _mockRepository.UpdateAsync(Arg.Any<RetailAccount>())
                .Returns(x => Task.FromResult(x.Arg<RetailAccount>()));

            // Act
            var result = await _accountService.WithdrawAsync(accountId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4000m, result.Balance);
            await _mockRepository.Received(1).GetByIdAsync(accountId);
            await _mockRepository.Received(1).UpdateAsync(Arg.Any<RetailAccount>());
        }

        [Fact]
        public async Task WithdrawAsync_WithInsufficientFunds_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var account = new RetailAccount
            {
                Id = accountId,
                CustomerName = "John Doe",
                AccountNumber = "ACC001",
                Balance = 500m,
                AccountType = AccountType.Checking
            };
            var request = new WithdrawRequest { Amount = 1000m };
            _mockRepository.GetByIdAsync(accountId).Returns(account);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _accountService.WithdrawAsync(accountId, request));
        }

        [Fact]
        public async Task WithdrawAsync_WithZeroAmount_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new WithdrawRequest { Amount = 0m };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _accountService.WithdrawAsync(accountId, request));
        }

        [Fact]
        public async Task WithdrawAsync_WithNegativeAmount_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new WithdrawRequest { Amount = -500m };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _accountService.WithdrawAsync(accountId, request));
        }

        [Fact]
        public async Task WithdrawAsync_WithNonexistentAccount_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var request = new WithdrawRequest { Amount = 1000m };
            _mockRepository.GetByIdAsync(accountId).Returns(default(RetailAccount));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _accountService.WithdrawAsync(accountId, request));
        }

        [Fact]
        public async Task CloseAsync_WithValidId_ShouldCloseAccount()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            _mockRepository.DeleteAsync(accountId).Returns(true);

            // Act
            var result = await _accountService.CloseAsync(accountId);

            // Assert
            Assert.True(result);
            await _mockRepository.Received(1).DeleteAsync(accountId);
        }

        [Fact]
        public async Task CloseAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            _mockRepository.DeleteAsync(accountId).Returns(false);

            // Act
            var result = await _accountService.CloseAsync(accountId);

            // Assert
            Assert.False(result);
            await _mockRepository.Received(1).DeleteAsync(accountId);
        }
    }
}
