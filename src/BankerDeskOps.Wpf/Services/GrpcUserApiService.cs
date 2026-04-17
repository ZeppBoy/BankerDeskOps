using BankerDeskOps.Api.Protos;
using BankerDeskOps.Application.DTOs;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    public class GrpcUserApiService
    {
        private readonly GrpcChannelManager _channelManager;
        private readonly ILogger<GrpcUserApiService> _logger;

        public GrpcUserApiService(GrpcChannelManager channelManager, ILogger<GrpcUserApiService> logger)
        {
            _channelManager = channelManager ?? throw new ArgumentNullException(nameof(channelManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Application.DTOs.LoginResponse> LoginAsync(string username, string password)
        {
            try
            {
                _logger.LogInformation("gRPC: Login attempt for {Username}", username);
                var client = new UserService.UserServiceClient(_channelManager.Channel);
                var response = await client.LoginAsync(new Api.Protos.LoginRequest
                {
                    Username = username,
                    Password = password ?? string.Empty
                });

                return new Application.DTOs.LoginResponse
                {
                    Success = response.Success,
                    IsAnonymous = response.IsAnonymous,
                    ErrorMessage = string.IsNullOrEmpty(response.ErrorMessage) ? null : response.ErrorMessage,
                    User = response.User is not null ? MapProtoToDto(response.User) : null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Login error: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            try
            {
                _logger.LogInformation("gRPC: Fetching all users");
                var client = new UserService.UserServiceClient(_channelManager.Channel);
                var response = await client.GetAllUsersAsync(new Empty());
                return response.Users.Select(MapProtoToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error fetching users: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<UserDto?> CreateUserAsync(Application.DTOs.CreateUserRequest request)
        {
            try
            {
                _logger.LogInformation("gRPC: Creating user {Username}", request.Username);
                var client = new UserService.UserServiceClient(_channelManager.Channel);
                var protoRequest = new Api.Protos.CreateUserRequest
                {
                    Username = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Password = request.Password,
                    Role = (UserRole)(int)request.Role
                };
                var response = await client.CreateUserAsync(protoRequest);
                return response.User is not null ? MapProtoToDto(response.User) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error creating user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<UserDto?> UpdateUserAsync(Guid id, Application.DTOs.UpdateUserRequest request)
        {
            try
            {
                _logger.LogInformation("gRPC: Updating user {UserId}", id);
                var client = new UserService.UserServiceClient(_channelManager.Channel);
                var protoRequest = new Api.Protos.UpdateUserRequest
                {
                    Id = id.ToString(),
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = (UserRole)(int)request.Role
                };
                var response = await client.UpdateUserAsync(protoRequest);
                return response.User is not null ? MapProtoToDto(response.User) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error updating user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<UserDto?> ActivateUserAsync(Guid id)
        {
            try
            {
                var client = new UserService.UserServiceClient(_channelManager.Channel);
                var response = await client.ActivateUserAsync(new ActivateUserRequest { Id = id.ToString() });
                return response.User is not null ? MapProtoToDto(response.User) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error activating user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<UserDto?> DeactivateUserAsync(Guid id)
        {
            try
            {
                var client = new UserService.UserServiceClient(_channelManager.Channel);
                var response = await client.DeactivateUserAsync(new DeactivateUserRequest { Id = id.ToString() });
                return response.User is not null ? MapProtoToDto(response.User) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error deactivating user: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            try
            {
                var client = new UserService.UserServiceClient(_channelManager.Channel);
                var response = await client.DeleteUserAsync(new DeleteUserRequest { Id = id.ToString() });
                return response.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError("gRPC: Error deleting user: {Message}", ex.Message);
                throw;
            }
        }

        private static UserDto MapProtoToDto(UserMessage proto) => new UserDto
        {
            Id = Guid.Parse(proto.Id),
            Username = proto.Username,
            Email = proto.Email,
            FirstName = proto.FirstName,
            LastName = proto.LastName,
            Role = (Domain.Enums.UserRole)(int)proto.Role,
            Status = (Domain.Enums.UserStatus)(int)proto.Status,
            LastLoginAt = string.IsNullOrEmpty(proto.LastLoginAt) ? null : DateTime.Parse(proto.LastLoginAt),
            CreatedAt = DateTime.Parse(proto.CreatedAt),
            UpdatedAt = DateTime.Parse(proto.UpdatedAt)
        };
    }
}
