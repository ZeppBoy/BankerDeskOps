using System.Net;
using System.Net.Http.Json;
using BankerDeskOps.Api;
using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BankerDeskOps.Api.Tests.Controllers
{
    public class LoansControllerTests : IAsyncLifetime
    {
        private readonly ApiTestWebApplicationFactory _factory;
        private HttpClient? _client;

        public LoansControllerTests()
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
        public async Task GetLoans_ShouldReturnOkWithLoans()
        {
            // Act
            var response = await _client!.GetAsync("/api/loans");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var loans = await response.Content.ReadFromJsonAsync<IEnumerable<LoanDto>>();
            Assert.NotNull(loans);
        }

        [Fact]
        public async Task CreateLoan_WithValidRequest_ShouldReturnCreated()
        {
            // Arrange
            var request = new CreateLoanRequest
            {
                CustomerName = "John Doe",
                Amount = 15000m,
                InterestRate = 5.5m,
                TermMonths = 12
            };

            // Act
            var response = await _client!.PostAsJsonAsync("/api/loans", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var loan = await response.Content.ReadFromJsonAsync<LoanDto>();
            Assert.NotNull(loan);
            Assert.Equal("John Doe", loan.CustomerName);
            Assert.Equal(15000m, loan.Amount);
        }

        [Fact]
        public async Task CreateLoan_WithInvalidAmount_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new CreateLoanRequest
            {
                CustomerName = "John Doe",
                Amount = 0m,
                InterestRate = 5.5m,
                TermMonths = 12
            };

            // Act
            var response = await _client!.PostAsJsonAsync("/api/loans", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetLoanById_WithValidId_ShouldReturnOk()
        {
            // Arrange: Create a loan first
            var createRequest = new CreateLoanRequest
            {
                CustomerName = "Jane Smith",
                Amount = 25000m,
                InterestRate = 4.5m,
                TermMonths = 24
            };
            var createResponse = await _client!.PostAsJsonAsync("/api/loans", createRequest);
            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDto>();

            // Act
            var response = await _client!.GetAsync($"/api/loans/{createdLoan!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var loan = await response.Content.ReadFromJsonAsync<LoanDto>();
            Assert.NotNull(loan);
            Assert.Equal("Jane Smith", loan.CustomerName);
        }

        [Fact]
        public async Task GetLoanById_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client!.GetAsync($"/api/loans/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ApproveLoan_WithPendingLoan_ShouldReturnOk()
        {
            // Arrange: Create a loan first
            var createRequest = new CreateLoanRequest
            {
                CustomerName = "Bob Wilson",
                Amount = 30000m,
                InterestRate = 6.0m,
                TermMonths = 36
            };
            var createResponse = await _client!.PostAsJsonAsync("/api/loans", createRequest);
            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDto>();

            // Act
            var response = await _client!.PutAsync($"/api/loans/{createdLoan!.Id}/approve", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var approvedLoan = await response.Content.ReadFromJsonAsync<LoanDto>();
            Assert.Equal(LoanStatus.Approved, approvedLoan!.Status);
        }

        [Fact]
        public async Task RejectLoan_WithPendingLoan_ShouldReturnOk()
        {
            // Arrange: Create a loan first
            var createRequest = new CreateLoanRequest
            {
                CustomerName = "Alice Brown",
                Amount = 5000m,
                InterestRate = 7.0m,
                TermMonths = 6
            };
            var createResponse = await _client!.PostAsJsonAsync("/api/loans", createRequest);
            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDto>();

            // Act
            var response = await _client!.PutAsync($"/api/loans/{createdLoan!.Id}/reject", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rejectedLoan = await response.Content.ReadFromJsonAsync<LoanDto>();
            Assert.Equal(LoanStatus.Rejected, rejectedLoan!.Status);
        }

        [Fact]
        public async Task DeleteLoan_WithValidId_ShouldReturnNoContent()
        {
            // Arrange: Create a loan first
            var createRequest = new CreateLoanRequest
            {
                CustomerName = "Charlie Davis",
                Amount = 12000m,
                InterestRate = 5.0m,
                TermMonths = 18
            };
            var createResponse = await _client!.PostAsJsonAsync("/api/loans", createRequest);
            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDto>();

            // Act
            var response = await _client!.DeleteAsync($"/api/loans/{createdLoan!.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify deletion
            var getResponse = await _client!.GetAsync($"/api/loans/{createdLoan!.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteLoan_WithInvalidId_ShouldReturnNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();

            // Act
            var response = await _client!.DeleteAsync($"/api/loans/{invalidId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
