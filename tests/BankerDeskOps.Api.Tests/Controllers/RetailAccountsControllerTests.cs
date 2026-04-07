using System.Net;
using System.Net.Http.Json;
using BankerDeskOps.Api;
using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BankerDeskOps.Api.Tests.Controllers
{
    public class RetailAccountsControllerTests : IAsyncLifetime
    {
        private readonly ApiTestWebApplicationFactory _factory;
        private HttpClient? _client;

        public RetailAccountsControllerTests()
        {
            _factory = new ApiTestWebApplicationFactory();
        }

        public async Task InitializeAsync()
        {
            _client = _factory.CreateClient();
            await Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            _factory?.Dispose();
            await Task.CompletedTask;
        }

        [Fact]
        public async Task GetAccounts_ShouldReturnOkWithAccounts()
        {
            // Act
            var response = await _client!.GetAsync("/api/retailaccounts");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var accounts = await response.Content.ReadFromJsonAsync<IEnumerable<RetailAccountDto>>();
            Assert.NotNull(accounts);
        }

        [Fact]
        public async Task CreateAccount_WithValidRequest_ShouldReturnCreated()
        {
            // Arrange
            var request = new CreateRetailAccountRequest
            {
                CustomerName = "John Doe",
                AccountType = AccountType.Checking,
                InitialDeposit = 0m
            };

            // Act
            var response = await _client!.PostAsJsonAsync("/api/retailaccounts", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var account = await response.Content.ReadFromJsonAsync<RetailAccountDto>();
            Assert.NotNull(account);
            Assert.Equal("John Doe", account.CustomerName);
            Assert.Equal(0m, account.Balance);
            Assert.NotEmpty(account.AccountNumber);
        }

        [Fact]
        public async Task CreateAccount_WithEmptyCustomerName_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateRetailAccountRequest
            {
                CustomerName = "",
                AccountType = AccountType.Savings,
                InitialDeposit = 0m
            };

            // Act
            var response = await _client!.PostAsJsonAsync("/api/retailaccounts", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetAccountById_WithValidId_ShouldReturnOk()
        {
            // Arrange: Create an account first
            var createRequest = new CreateRetailAccountRequest
            {
                CustomerName = "Jane Smith",
                AccountType = AccountType.Savings,
                InitialDeposit = 0m
            };
            var createResponse = await _client!.PostAsJsonAsync("/api/retailaccounts", createRequest);
            var createdAccount = await createResponse.Content.ReadFromJsonAsync<RetailAccountDto>();

            // Act
            var response = await _client!.GetAsync($"/api/retailaccounts/{createdAccount!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var account = await response.Content.ReadFromJsonAsync<RetailAccountDto>();
            Assert.NotNull(account);
            Assert.Equal("Jane Smith", account.CustomerName);
        }

        [Fact]
        public async Task GetAccountById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client!.GetAsync($"/api/retailaccounts/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DepositAsync_WithValidAmount_ShouldReturnOk()
        {
            // Arrange: Create an account first
            var createRequest = new CreateRetailAccountRequest
            {
                CustomerName = "Bob Wilson",
                AccountType = AccountType.Checking,
                InitialDeposit = 0m
            };
            var createResponse = await _client!.PostAsJsonAsync("/api/retailaccounts", createRequest);
            var createdAccount = await createResponse.Content.ReadFromJsonAsync<RetailAccountDto>();

            var depositRequest = new DepositRequest { Amount = 5000m };

            // Act
            var response = await _client!.PostAsJsonAsync(
                $"/api/retailaccounts/{createdAccount!.Id}/deposit",
                depositRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updatedAccount = await response.Content.ReadFromJsonAsync<RetailAccountDto>();
            Assert.Equal(5000m, updatedAccount!.Balance);
        }

        [Fact]
        public async Task DepositAsync_WithZeroAmount_ShouldReturnBadRequest()
        {
            // Arrange: Create an account first
            var createRequest = new CreateRetailAccountRequest
            {
                CustomerName = "Alice Brown",
                AccountType = AccountType.Savings,
                InitialDeposit = 0m
            };
            var createResponse = await _client!.PostAsJsonAsync("/api/retailaccounts", createRequest);
            var createdAccount = await createResponse.Content.ReadFromJsonAsync<RetailAccountDto>();

            var depositRequest = new DepositRequest { Amount = 0m };

            // Act
            var response = await _client!.PostAsJsonAsync(
                $"/api/retailaccounts/{createdAccount!.Id}/deposit",
                depositRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DepositAsync_WithNegativeAmount_ShouldReturnBadRequest()
        {
            // Arrange: Create an account first
            var createRequest = new CreateRetailAccountRequest
            {
                CustomerName = "Charlie Davis",
                AccountType = AccountType.Checking,
                InitialDeposit = 0m
            };
            var createResponse = await _client!.PostAsJsonAsync("/api/retailaccounts", createRequest);
            var createdAccount = await createResponse.Content.ReadFromJsonAsync<RetailAccountDto>();

            var depositRequest = new DepositRequest { Amount = -1000m };

            // Act
            var response = await _client!.PostAsJsonAsync(
                $"/api/retailaccounts/{createdAccount!.Id}/deposit",
                depositRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task WithdrawAsync_WithValidAmount_ShouldReturnOk()
        {
            // Arrange: Create an account and deposit funds first
            var createRequest = new CreateRetailAccountRequest
            {
                CustomerName = "Diana Evans",
                AccountType = AccountType.Checking,
                InitialDeposit = 10000m
            };
            var createResponse = await _client!.PostAsJsonAsync("/api/retailaccounts", createRequest);
            var createdAccount = await createResponse.Content.ReadFromJsonAsync<RetailAccountDto>();

            var withdrawRequest = new WithdrawRequest { Amount = 3000m };

            // Act
            var response = await _client!.PostAsJsonAsync(
                $"/api/retailaccounts/{createdAccount!.Id}/withdraw",
                withdrawRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updatedAccount = await response.Content.ReadFromJsonAsync<RetailAccountDto>();
            Assert.Equal(7000m, updatedAccount!.Balance);
        }

        [Fact]
        public async Task WithdrawAsync_WithInsufficientFunds_ShouldReturnBadRequest()
        {
            // Arrange: Create an account with limited funds
            var createRequest = new CreateRetailAccountRequest
            {
                CustomerName = "Edward Frank",
                AccountType = AccountType.Savings,
                InitialDeposit = 1000m
            };
            var createResponse = await _client!.PostAsJsonAsync("/api/retailaccounts", createRequest);
            var createdAccount = await createResponse.Content.ReadFromJsonAsync<RetailAccountDto>();

            var withdrawRequest = new WithdrawRequest { Amount = 5000m };

            // Act
            var response = await _client!.PostAsJsonAsync(
                $"/api/retailaccounts/{createdAccount!.Id}/withdraw",
                withdrawRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task WithdrawAsync_WithZeroAmount_ShouldReturnBadRequest()
        {
            // Arrange: Create an account first
            var createRequest = new CreateRetailAccountRequest
            {
                CustomerName = "Fiona Gray",
                AccountType = AccountType.Checking,
                InitialDeposit = 0m
            };
            var createResponse = await _client!.PostAsJsonAsync("/api/retailaccounts", createRequest);
            var createdAccount = await createResponse.Content.ReadFromJsonAsync<RetailAccountDto>();

            var withdrawRequest = new WithdrawRequest { Amount = 0m };

            // Act
            var response = await _client!.PostAsJsonAsync(
                $"/api/retailaccounts/{createdAccount!.Id}/withdraw",
                withdrawRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task WithdrawAsync_WithNegativeAmount_ShouldReturnBadRequest()
        {
            // Arrange: Create an account first
            var createRequest = new CreateRetailAccountRequest
            {
                CustomerName = "Greg Harris",
                AccountType = AccountType.Savings,
                InitialDeposit = 0m
            };
            var createResponse = await _client!.PostAsJsonAsync("/api/retailaccounts", createRequest);
            var createdAccount = await createResponse.Content.ReadFromJsonAsync<RetailAccountDto>();

            var withdrawRequest = new WithdrawRequest { Amount = -500m };

            // Act
            var response = await _client!.PostAsJsonAsync(
                $"/api/retailaccounts/{createdAccount!.Id}/withdraw",
                withdrawRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteAccount_WithValidId_ShouldReturnNoContent()
        {
            // Arrange: Create an account first
            var createRequest = new CreateRetailAccountRequest
            {
                CustomerName = "Helen Iverson",
                AccountType = AccountType.Checking,
                InitialDeposit = 0m
            };
            var createResponse = await _client!.PostAsJsonAsync("/api/retailaccounts", createRequest);
            var createdAccount = await createResponse.Content.ReadFromJsonAsync<RetailAccountDto>();

            // Act
            var response = await _client!.DeleteAsync($"/api/retailaccounts/{createdAccount!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify deletion
            var getResponse = await _client!.GetAsync($"/api/retailaccounts/{createdAccount!.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteAccount_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client!.DeleteAsync($"/api/retailaccounts/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
