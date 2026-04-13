using System.Net;
using System.Net.Http.Json;
using BankerDeskOps.Api;
using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Domain.Enums;
using Xunit;

namespace BankerDeskOps.Api.Tests.Controllers
{
    public class BankClientsControllerTests : IAsyncLifetime
    {
        private readonly ApiTestWebApplicationFactory _factory;
        private HttpClient? _client;

        public BankClientsControllerTests()
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

        // ── Helpers ───────────────────────────────────────────────────────────

        private static CreateBankClientRequest ValidCreateRequest(string email = "john.doe@example.com",
            string nationalId = "ID001") => new CreateBankClientRequest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = email,
                PhoneNumber = "+48123456789",
                DateOfBirth = new DateTime(1985, 6, 15),
                NationalId = nationalId,
                Street = "Main St 1",
                City = "Warsaw",
                PostalCode = "00-001",
                Country = "Poland"
            };

        private async Task<BankClientDto> CreateClientAsync(string email = "john.doe@example.com",
            string nationalId = "ID001")
        {
            var response = await _client!.PostAsJsonAsync("/api/bankclients", ValidCreateRequest(email, nationalId));
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<BankClientDto>())!;
        }

        // ── GET /api/bankclients ──────────────────────────────────────────────

        [Fact]
        public async Task GetClients_ShouldReturnOkWithClients()
        {
            var response = await _client!.GetAsync("/api/bankclients");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var clients = await response.Content.ReadFromJsonAsync<IEnumerable<BankClientDto>>();
            Assert.NotNull(clients);
        }

        // ── POST /api/bankclients ─────────────────────────────────────────────

        [Fact]
        public async Task CreateClient_WithValidRequest_ShouldReturnCreated()
        {
            var request = ValidCreateRequest("create_test@example.com", "NATID_CT");

            var response = await _client!.PostAsJsonAsync("/api/bankclients", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var client = await response.Content.ReadFromJsonAsync<BankClientDto>();
            Assert.NotNull(client);
            Assert.Equal("John", client!.FirstName);
            Assert.Equal("create_test@example.com", client.Email);
            Assert.Equal(ClientStatus.Active, client.Status);
        }

        [Fact]
        public async Task CreateClient_WithMissingFirstName_ShouldReturnBadRequest()
        {
            var request = ValidCreateRequest("bad_test@example.com", "NATID_BT");
            request.FirstName = "   ";

            var response = await _client!.PostAsJsonAsync("/api/bankclients", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ── GET /api/bankclients/{id} ─────────────────────────────────────────

        [Fact]
        public async Task GetClientById_WithValidId_ShouldReturnOk()
        {
            var created = await CreateClientAsync("getbyid@example.com", "NATID_GBI");

            var response = await _client!.GetAsync($"/api/bankclients/{created.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var client = await response.Content.ReadFromJsonAsync<BankClientDto>();
            Assert.NotNull(client);
            Assert.Equal(created.Id, client!.Id);
        }

        [Fact]
        public async Task GetClientById_WithInvalidId_ShouldReturnNotFound()
        {
            var response = await _client!.GetAsync($"/api/bankclients/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ── PUT /api/bankclients/{id} ─────────────────────────────────────────

        [Fact]
        public async Task UpdateClient_WithValidRequest_ShouldReturnOk()
        {
            var created = await CreateClientAsync("update_test@example.com", "NATID_UT");

            var updateRequest = new UpdateBankClientRequest
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "update_test@example.com",
                DateOfBirth = new DateTime(1990, 3, 20),
                NationalId = "NATID_UT",
                City = "Krakow",
                Country = "Poland"
            };

            var response = await _client!.PutAsJsonAsync($"/api/bankclients/{created.Id}", updateRequest);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<BankClientDto>();
            Assert.NotNull(updated);
            Assert.Equal("Jane", updated!.FirstName);
            Assert.Equal("Krakow", updated.City);
        }

        // ── PUT /api/bankclients/{id}/suspend ─────────────────────────────────

        [Fact]
        public async Task SuspendClient_WithActiveClient_ShouldReturnOkWithSuspendedStatus()
        {
            var created = await CreateClientAsync("suspend_test@example.com", "NATID_ST");

            var response = await _client!.PutAsync($"/api/bankclients/{created.Id}/suspend", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<BankClientDto>();
            Assert.Equal(ClientStatus.Suspended, updated!.Status);
        }

        // ── PUT /api/bankclients/{id}/activate ────────────────────────────────

        [Fact]
        public async Task ActivateClient_WithSuspendedClient_ShouldReturnOkWithActiveStatus()
        {
            var created = await CreateClientAsync("activate_test@example.com", "NATID_AT");
            await _client!.PutAsync($"/api/bankclients/{created.Id}/suspend", null);

            var response = await _client!.PutAsync($"/api/bankclients/{created.Id}/activate", null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var updated = await response.Content.ReadFromJsonAsync<BankClientDto>();
            Assert.Equal(ClientStatus.Active, updated!.Status);
        }

        // ── DELETE /api/bankclients/{id} ──────────────────────────────────────

        [Fact]
        public async Task DeleteClient_WithValidId_ShouldReturnNoContent()
        {
            var created = await CreateClientAsync("delete_test@example.com", "NATID_DT");

            var deleteResponse = await _client!.DeleteAsync($"/api/bankclients/{created.Id}");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await _client!.GetAsync($"/api/bankclients/{created.Id}");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteClient_WithInvalidId_ShouldReturnNotFound()
        {
            var response = await _client!.DeleteAsync($"/api/bankclients/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
