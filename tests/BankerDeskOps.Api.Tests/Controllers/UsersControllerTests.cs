using System.Net;
using System.Net.Http.Json;
using BankerDeskOps.Api;
using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Domain.Enums;
using Xunit;

namespace BankerDeskOps.Api.Tests.Controllers
{
    public class UsersControllerTests : IAsyncLifetime
    {
        private readonly ApiTestWebApplicationFactory _factory;
        private HttpClient? _client;

        public UsersControllerTests()
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

        private static CreateUserRequest ValidCreateRequest(string username = "testuser",
            string email = "testuser@example.com") => new CreateUserRequest
            {
                Username = username,
                Email = email,
                FirstName = "Test",
                LastName = "User",
                Password = "Password1",
                Role = UserRole.Operator
            };

        private async Task<UserDto> CreateUserAsync(string username = "testuser",
            string email = "testuser@example.com")
        {
            var response = await _client!.PostAsJsonAsync("/api/users", ValidCreateRequest(username, email));
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<UserDto>())!;
        }

        // ── GET /api/users ────────────────────────────────────────────────────

        [Fact]
        public async Task GetUsers_ShouldReturnOk()
        {
            var response = await _client!.GetAsync("/api/users");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
            Assert.NotNull(users);
        }

        // ── POST /api/users ───────────────────────────────────────────────────

        [Fact]
        public async Task CreateUser_WithValidRequest_ShouldReturnCreated()
        {
            var request = ValidCreateRequest("create_test", "create_test@example.com");

            var response = await _client!.PostAsJsonAsync("/api/users", request);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.NotNull(user);
            Assert.Equal("create_test", user!.Username);
            Assert.Equal(UserStatus.Active, user.Status);
        }

        [Fact]
        public async Task CreateUser_WithShortPassword_ShouldReturnBadRequest()
        {
            var request = ValidCreateRequest("badpwd", "badpwd@example.com");
            request.Password = "short";

            var response = await _client!.PostAsJsonAsync("/api/users", request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateUser_DuplicateUsername_ShouldReturnConflict()
        {
            await CreateUserAsync("dup_user", "dup1@example.com");

            var request = ValidCreateRequest("dup_user", "dup2@example.com");
            var response = await _client!.PostAsJsonAsync("/api/users", request);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // ── GET /api/users/{id} ───────────────────────────────────────────────

        [Fact]
        public async Task GetUserById_WithValidId_ShouldReturnOk()
        {
            var created = await CreateUserAsync("getbyid", "getbyid@example.com");

            var response = await _client!.GetAsync($"/api/users/{created.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.Equal("getbyid", user!.Username);
        }

        [Fact]
        public async Task GetUserById_WithInvalidId_ShouldReturnNotFound()
        {
            var response = await _client!.GetAsync($"/api/users/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ── PUT /api/users/{id} ───────────────────────────────────────────────

        [Fact]
        public async Task UpdateUser_WithValidRequest_ShouldReturnOk()
        {
            var created = await CreateUserAsync("upd_user", "upd_user@example.com");

            var updateReq = new UpdateUserRequest
            {
                Email = "updated@example.com",
                FirstName = "Updated",
                LastName = "Name",
                Role = UserRole.Manager
            };

            var response = await _client!.PutAsJsonAsync($"/api/users/{created.Id}", updateReq);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            Assert.Equal("Updated", user!.FirstName);
            Assert.Equal(UserRole.Manager, user.Role);
        }

        // ── DELETE /api/users/{id} ────────────────────────────────────────────

        [Fact]
        public async Task DeleteUser_WithValidId_ShouldReturnNoContent()
        {
            var created = await CreateUserAsync("del_user", "del_user@example.com");

            var response = await _client!.DeleteAsync($"/api/users/{created.Id}");

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_WithInvalidId_ShouldReturnNotFound()
        {
            var response = await _client!.DeleteAsync($"/api/users/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // ── POST /api/users/login ─────────────────────────────────────────────

        [Fact]
        public async Task Login_WithValidCredentials_ShouldSucceed()
        {
            await CreateUserAsync("login_user", "login_user@example.com");

            var loginReq = new LoginRequest { Username = "login_user", Password = "Password1" };
            var response = await _client!.PostAsJsonAsync("/api/users/login", loginReq);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.True(loginResponse!.Success);
            Assert.NotNull(loginResponse.User);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ShouldReturnFailure()
        {
            await CreateUserAsync("login_fail", "login_fail@example.com");

            var loginReq = new LoginRequest { Username = "login_fail", Password = "WrongPassword" };
            var response = await _client!.PostAsJsonAsync("/api/users/login", loginReq);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.False(loginResponse!.Success);
        }

        [Fact]
        public async Task Login_Anonymous_ShouldSucceed()
        {
            var loginReq = new LoginRequest { Username = "anonymous", Password = "" };
            var response = await _client!.PostAsJsonAsync("/api/users/login", loginReq);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.True(loginResponse!.Success);
            Assert.True(loginResponse.IsAnonymous);
        }
    }
}
