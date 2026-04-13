using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Application.Services;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Domain.Enums;
using NSubstitute;
using Xunit;

namespace BankerDeskOps.Application.Tests.Services
{
    public class BankClientServiceTests
    {
        private readonly IBankClientRepository _mockRepository;
        private readonly BankClientService _service;

        public BankClientServiceTests()
        {
            _mockRepository = Substitute.For<IBankClientRepository>();
            _service = new BankClientService(_mockRepository);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static BankClient MakeClient(Guid? id = null, string email = "john@example.com",
            ClientStatus status = ClientStatus.Active) => new BankClient
            {
                Id = id ?? Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = email,
                PhoneNumber = "+1234567890",
                DateOfBirth = new DateTime(1985, 6, 15),
                NationalId = "ID123456",
                Street = "Main St 1",
                City = "Warsaw",
                PostalCode = "00-001",
                Country = "Poland",
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        private static CreateBankClientRequest MakeCreateRequest(string email = "john@example.com",
            string firstName = "John", string nationalId = "ID123456") => new CreateBankClientRequest
            {
                FirstName = firstName,
                LastName = "Doe",
                Email = email,
                PhoneNumber = "+1234567890",
                DateOfBirth = new DateTime(1985, 6, 15),
                NationalId = nationalId,
                City = "Warsaw",
                Country = "Poland"
            };

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllClients()
        {
            var clients = new List<BankClient> { MakeClient(), MakeClient(email: "jane@example.com") };
            _mockRepository.GetAllAsync().Returns(clients);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            await _mockRepository.Received(1).GetAllAsync();
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnClient()
        {
            var id = Guid.NewGuid();
            var client = MakeClient(id);
            _mockRepository.GetByIdAsync(id).Returns(client);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(id, result!.Id);
            Assert.Equal("John", result.FirstName);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            var id = Guid.NewGuid();
            _mockRepository.GetByIdAsync(id).Returns(default(BankClient));

            var result = await _service.GetByIdAsync(id);

            Assert.Null(result);
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_WithValidRequest_ShouldCreateClient()
        {
            var request = MakeCreateRequest();
            _mockRepository.GetByEmailAsync(Arg.Any<string>()).Returns(default(BankClient));
            _mockRepository.GetByNationalIdAsync(Arg.Any<string>()).Returns(default(BankClient));
            _mockRepository.CreateAsync(Arg.Any<BankClient>())
                .Returns(x => Task.FromResult(x.Arg<BankClient>()));

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
            Assert.Equal(ClientStatus.Active, result.Status);
            Assert.NotEqual(Guid.Empty, result.Id);
            await _mockRepository.Received(1).CreateAsync(Arg.Any<BankClient>());
        }

        [Fact]
        public async Task CreateAsync_WithEmptyFirstName_ShouldThrowArgumentException()
        {
            var request = MakeCreateRequest();
            request.FirstName = "   ";

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_WithInvalidEmail_ShouldThrowArgumentException()
        {
            var request = MakeCreateRequest(email: "not-an-email");

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateEmail_ShouldThrowInvalidOperationException()
        {
            var request = MakeCreateRequest();
            _mockRepository.GetByEmailAsync(Arg.Any<string>()).Returns(MakeClient());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateNationalId_ShouldThrowInvalidOperationException()
        {
            var request = MakeCreateRequest();
            _mockRepository.GetByEmailAsync(Arg.Any<string>()).Returns(default(BankClient));
            _mockRepository.GetByNationalIdAsync(Arg.Any<string>()).Returns(MakeClient());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_WithValidRequest_ShouldUpdateClient()
        {
            var id = Guid.NewGuid();
            var existing = MakeClient(id);
            _mockRepository.GetByIdAsync(id).Returns(existing);
            _mockRepository.GetByEmailAsync(Arg.Any<string>()).Returns(default(BankClient));
            _mockRepository.GetByNationalIdAsync(Arg.Any<string>()).Returns(default(BankClient));
            _mockRepository.UpdateAsync(Arg.Any<BankClient>())
                .Returns(x => Task.FromResult(x.Arg<BankClient>()));

            var updateRequest = new UpdateBankClientRequest
            {
                FirstName = "Jane", LastName = "Doe", Email = "jane@example.com",
                DateOfBirth = new DateTime(1990, 1, 1), NationalId = "ID999"
            };

            var result = await _service.UpdateAsync(id, updateRequest);

            Assert.Equal("Jane", result.FirstName);
            await _mockRepository.Received(1).UpdateAsync(Arg.Any<BankClient>());
        }

        [Fact]
        public async Task UpdateAsync_WithNonexistentId_ShouldThrowInvalidOperationException()
        {
            var id = Guid.NewGuid();
            _mockRepository.GetByIdAsync(id).Returns(default(BankClient));

            var request = new UpdateBankClientRequest
            {
                FirstName = "Jane", LastName = "Doe", Email = "jane@example.com",
                DateOfBirth = new DateTime(1990, 1, 1), NationalId = "ID999"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(id, request));
        }

        // ── SuspendAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task SuspendAsync_WithActiveClient_ShouldChangeStatusToSuspended()
        {
            var id = Guid.NewGuid();
            var client = MakeClient(id, status: ClientStatus.Active);
            _mockRepository.GetByIdAsync(id).Returns(client);
            _mockRepository.UpdateAsync(Arg.Any<BankClient>())
                .Returns(x => Task.FromResult(x.Arg<BankClient>()));

            var result = await _service.SuspendAsync(id);

            Assert.Equal(ClientStatus.Suspended, result.Status);
        }

        [Fact]
        public async Task SuspendAsync_WithAlreadySuspendedClient_ShouldThrowInvalidOperationException()
        {
            var id = Guid.NewGuid();
            _mockRepository.GetByIdAsync(id).Returns(MakeClient(id, status: ClientStatus.Suspended));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SuspendAsync(id));
        }

        [Fact]
        public async Task SuspendAsync_WithNonexistentClient_ShouldThrowInvalidOperationException()
        {
            var id = Guid.NewGuid();
            _mockRepository.GetByIdAsync(id).Returns(default(BankClient));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SuspendAsync(id));
        }

        // ── ActivateAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task ActivateAsync_WithSuspendedClient_ShouldChangeStatusToActive()
        {
            var id = Guid.NewGuid();
            var client = MakeClient(id, status: ClientStatus.Suspended);
            _mockRepository.GetByIdAsync(id).Returns(client);
            _mockRepository.UpdateAsync(Arg.Any<BankClient>())
                .Returns(x => Task.FromResult(x.Arg<BankClient>()));

            var result = await _service.ActivateAsync(id);

            Assert.Equal(ClientStatus.Active, result.Status);
        }

        [Fact]
        public async Task ActivateAsync_WithAlreadyActiveClient_ShouldThrowInvalidOperationException()
        {
            var id = Guid.NewGuid();
            _mockRepository.GetByIdAsync(id).Returns(MakeClient(id, status: ClientStatus.Active));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ActivateAsync(id));
        }

        // ── DeleteAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldReturnTrue()
        {
            var id = Guid.NewGuid();
            _mockRepository.DeleteAsync(id).Returns(true);

            var result = await _service.DeleteAsync(id);

            Assert.True(result);
            await _mockRepository.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
        {
            var id = Guid.NewGuid();
            _mockRepository.DeleteAsync(id).Returns(false);

            var result = await _service.DeleteAsync(id);

            Assert.False(result);
        }
    }
}
