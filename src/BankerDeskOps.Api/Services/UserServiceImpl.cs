using BankerDeskOps.Api.Protos;
using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace BankerDeskOps.Api.Services
{
    public class UserServiceImpl : UserService.UserServiceBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserServiceImpl> _logger;

        public UserServiceImpl(IUserService userService, ILogger<UserServiceImpl> logger)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<GetAllUsersResponse> GetAllUsers(Empty request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Fetching all users");
            var users = await _userService.GetAllAsync();
            var response = new GetAllUsersResponse();
            foreach (var user in users)
                response.Users.Add(MapUserToProto(user));
            return response;
        }

        public override async Task<GetUserByIdResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Fetching user with ID: {UserId}", request.Id);
            var user = await _userService.GetByIdAsync(Guid.Parse(request.Id));

            if (user is null)
            {
                _logger.LogWarning("gRPC: User with ID {UserId} not found", request.Id);
                throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.Id} not found"));
            }

            return new GetUserByIdResponse { User = MapUserToProto(user) };
        }

        public override async Task<CreateUserResponse> CreateUser(Protos.CreateUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Creating user: {Username}", request.Username);

            try
            {
                var createRequest = new Application.DTOs.CreateUserRequest
                {
                    Username = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Password = request.Password,
                    Role = (Domain.Enums.UserRole)(int)request.Role
                };

                var created = await _userService.CreateAsync(createRequest);
                return new CreateUserResponse { User = MapUserToProto(created) };
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("gRPC: Invalid user request: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("gRPC: User conflict: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
            }
        }

        public override async Task<UpdateUserResponse> UpdateUser(Protos.UpdateUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Updating user with ID: {UserId}", request.Id);

            try
            {
                var updateRequest = new Application.DTOs.UpdateUserRequest
                {
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Role = (Domain.Enums.UserRole)(int)request.Role
                };

                var updated = await _userService.UpdateAsync(Guid.Parse(request.Id), updateRequest);
                return new UpdateUserResponse { User = MapUserToProto(updated) };
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("gRPC: Invalid user update: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("gRPC: User update failed: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
        }

        public override async Task<ActivateUserResponse> ActivateUser(ActivateUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Activating user with ID: {UserId}", request.Id);

            try
            {
                var updated = await _userService.ActivateAsync(Guid.Parse(request.Id));
                return new ActivateUserResponse { User = MapUserToProto(updated) };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("gRPC: Activate user failed: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
        }

        public override async Task<DeactivateUserResponse> DeactivateUser(DeactivateUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Deactivating user with ID: {UserId}", request.Id);

            try
            {
                var updated = await _userService.DeactivateAsync(Guid.Parse(request.Id));
                return new DeactivateUserResponse { User = MapUserToProto(updated) };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("gRPC: Deactivate user failed: {Message}", ex.Message);
                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
        }

        public override async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Deleting user with ID: {UserId}", request.Id);
            var success = await _userService.DeleteAsync(Guid.Parse(request.Id));
            return new DeleteUserResponse { Success = success };
        }

        public override async Task<Protos.LoginResponse> Login(Protos.LoginRequest request, ServerCallContext context)
        {
            _logger.LogInformation("gRPC: Login attempt for user: {Username}", request.Username);

            var loginRequest = new Application.DTOs.LoginRequest
            {
                Username = request.Username,
                Password = request.Password
            };

            var result = await _userService.AuthenticateAsync(loginRequest);

            var response = new Protos.LoginResponse
            {
                Success = result.Success,
                IsAnonymous = result.IsAnonymous,
                ErrorMessage = result.ErrorMessage ?? string.Empty
            };

            if (result.User is not null)
                response.User = MapUserToProto(result.User);

            return response;
        }

        private static UserMessage MapUserToProto(UserDto user) => new UserMessage
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = (UserRole)(int)user.Role,
            Status = (UserStatus)(int)user.Status,
            LastLoginAt = user.LastLoginAt?.ToString("o") ?? string.Empty,
            CreatedAt = user.CreatedAt.ToString("o"),
            UpdatedAt = user.UpdatedAt.ToString("o")
        };
    }
}
