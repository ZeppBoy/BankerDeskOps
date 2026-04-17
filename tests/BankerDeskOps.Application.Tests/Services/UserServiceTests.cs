using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Application.Services;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Domain.Enums;
using NSubstitute;
using Xunit;

namespace BankerDeskOps.Application.Tests.Services
{
    public class UserServiceTests
    {
        private readonly IUserRepository _mockRepository;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _mockRepository = Substitute.For<IUserRepository>();
            _service = new UserService(_mockRepository);
        }

        private static User MakeUser(Guid? id = null, string username = "jdoe",
            string email = "jdoe@example.com", UserStatus status = UserStatus.Active) => new User
            {
                Id = id ?? Guid.NewGuid(),
                Username = username,
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1"),
                Role = UserRole.Operator,
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

        private static CreateUserRequest MakeCreateRequest(string username = "jdoe",
            string email = "jdoe@example.com") => new CreateUserRequest
            {
                Username = username,
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                Password = "Password1",
                Role = UserRole.Operator
            };

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            var users = new List<User> { MakeUser(), MakeUser(username: "jane", email: "jane@example.com") };
            _mockRepository.GetAllAsync().Returns(users);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            await _mockRepository.Received(1).GetAllAsync();
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
        {
            var id = Guid.NewGuid();
            var user = MakeUser(id);
            _mockRepository.GetByIdAsync(id).Returns(user);

            var result = await _service.GetByIdAsync(id);

            Assert.NotNull(result);
            Assert.Equal(id, result!.Id);
            Assert.Equal("jdoe", result.Username);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            var id = Guid.NewGuid();
            _mockRepository.GetByIdAsync(id).Returns(default(User));

            var result = await _service.GetByIdAsync(id);

            Assert.Null(result);
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_WithValidRequest_ShouldReturnUser()
        {
            var request = MakeCreateRequest("newuser", "newuser@example.com");
            _mockRepository.GetByUsernameAsync("newuser").Returns(default(User));
            _mockRepository.GetByEmailAsync("newuser@example.com").Returns(default(User));
            _mockRepository.CreateAsync(Arg.Any<User>()).Returns(ci => ci.Arg<User>());

            var result = await _service.CreateAsync(request);

            Assert.NotNull(result);
            Assert.Equal("newuser", result.Username);
            Assert.Equal("newuser@example.com", result.Email);
            await _mockRepository.Received(1).CreateAsync(Arg.Any<User>());
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateUsername_ShouldThrow()
        {
            var request = MakeCreateRequest();
            _mockRepository.GetByUsernameAsync("jdoe").Returns(MakeUser());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateEmail_ShouldThrow()
        {
            var request = MakeCreateRequest();
            _mockRepository.GetByUsernameAsync("jdoe").Returns(default(User));
            _mockRepository.GetByEmailAsync("jdoe@example.com").Returns(MakeUser());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_WithShortPassword_ShouldThrow()
        {
            var request = MakeCreateRequest();
            request.Password = "short";

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_WithEmptyUsername_ShouldThrow()
        {
            var request = MakeCreateRequest();
            request.Username = "  ";

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(request));
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_WithValidRequest_ShouldReturnUpdated()
        {
            var id = Guid.NewGuid();
            var user = MakeUser(id);
            _mockRepository.GetByIdAsync(id).Returns(user);
            _mockRepository.GetByEmailAsync("new@example.com").Returns(default(User));
            _mockRepository.UpdateAsync(Arg.Any<User>()).Returns(ci => ci.Arg<User>());

            var request = new UpdateUserRequest
            {
                Email = "new@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                Role = UserRole.Manager
            };

            var result = await _service.UpdateAsync(id, request);

            Assert.Equal("Jane", result.FirstName);
            Assert.Equal("new@example.com", result.Email);
            Assert.Equal(UserRole.Manager, result.Role);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ShouldThrow()
        {
            var id = Guid.NewGuid();
            _mockRepository.GetByIdAsync(id).Returns(default(User));

            var request = new UpdateUserRequest
            {
                Email = "x@x.com",
                FirstName = "X",
                LastName = "Y"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(id, request));
        }

        // ── ActivateAsync / DeactivateAsync ───────────────────────────────────

        [Fact]
        public async Task ActivateAsync_WithInactiveUser_ShouldActivate()
        {
            var id = Guid.NewGuid();
            _mockRepository.GetByIdAsync(id).Returns(MakeUser(id, status: UserStatus.Inactive));
            _mockRepository.UpdateAsync(Arg.Any<User>()).Returns(ci => ci.Arg<User>());

            var result = await _service.ActivateAsync(id);

            Assert.Equal(UserStatus.Active, result.Status);
        }

        [Fact]
        public async Task ActivateAsync_WithAlreadyActiveUser_ShouldThrow()
        {
            var id = Guid.NewGuid();
            _mockRepository.GetByIdAsync(id).Returns(MakeUser(id, status: UserStatus.Active));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ActivateAsync(id));
        }

        [Fact]
        public async Task DeactivateAsync_WithActiveUser_ShouldDeactivate()
        {
            var id = Guid.NewGuid();
            _mockRepository.GetByIdAsync(id).Returns(MakeUser(id, status: UserStatus.Active));
            _mockRepository.UpdateAsync(Arg.Any<User>()).Returns(ci => ci.Arg<User>());

            var result = await _service.DeactivateAsync(id);

            Assert.Equal(UserStatus.Inactive, result.Status);
        }

        // ── AuthenticateAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AuthenticateAsync_WithValidCredentials_ShouldSucceed()
        {
            var user = MakeUser();
            _mockRepository.GetByUsernameAsync("jdoe").Returns(user);
            _mockRepository.UpdateAsync(Arg.Any<User>()).Returns(ci => ci.Arg<User>());

            var result = await _service.AuthenticateAsync(new LoginRequest
            {
                Username = "jdoe",
                Password = "Password1"
            });

            Assert.True(result.Success);
            Assert.False(result.IsAnonymous);
            Assert.NotNull(result.User);
        }

        [Fact]
        public async Task AuthenticateAsync_WithInvalidPassword_ShouldFail()
        {
            var user = MakeUser();
            _mockRepository.GetByUsernameAsync("jdoe").Returns(user);

            var result = await _service.AuthenticateAsync(new LoginRequest
            {
                Username = "jdoe",
                Password = "WrongPassword"
            });

            Assert.False(result.Success);
            Assert.Equal("Invalid credentials.", result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_WithLockedUser_ShouldFail()
        {
            var user = MakeUser(status: UserStatus.Locked);
            _mockRepository.GetByUsernameAsync("jdoe").Returns(user);

            var result = await _service.AuthenticateAsync(new LoginRequest
            {
                Username = "jdoe",
                Password = "Password1"
            });

            Assert.False(result.Success);
            Assert.Equal("Account is locked.", result.ErrorMessage);
        }

        [Fact]
        public async Task AuthenticateAsync_Anonymous_ShouldSucceed()
        {
            var result = await _service.AuthenticateAsync(new LoginRequest
            {
                Username = "anonymous",
                Password = ""
            });

            Assert.True(result.Success);
            Assert.True(result.IsAnonymous);
            Assert.Equal("anonymous", result.User!.Username);
        }

        // ── DeleteAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_WithExistingUser_ShouldReturnTrue()
        {
            var id = Guid.NewGuid();
            _mockRepository.DeleteAsync(id).Returns(true);

            var result = await _service.DeleteAsync(id);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentUser_ShouldReturnFalse()
        {
            var id = Guid.NewGuid();
            _mockRepository.DeleteAsync(id).Returns(false);

            var result = await _service.DeleteAsync(id);

            Assert.False(result);
        }
    }
}
