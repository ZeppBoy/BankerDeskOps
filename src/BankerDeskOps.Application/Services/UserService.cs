using System.Net.Mail;
using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Application.Interfaces;
using BankerDeskOps.Domain.Entities;
using BankerDeskOps.Domain.Enums;

namespace BankerDeskOps.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user is null ? null : MapToDto(user);
        }

        public async Task<UserDto> CreateAsync(CreateUserRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            ValidateName(request.FirstName, nameof(request.FirstName));
            ValidateName(request.LastName, nameof(request.LastName));
            ValidateUsername(request.Username);
            ValidateEmail(request.Email);
            ValidatePassword(request.Password);

            var existingByUsername = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingByUsername is not null)
                throw new InvalidOperationException($"A user with username '{request.Username}' already exists.");

            var existingByEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingByEmail is not null)
                throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username.Trim().ToLowerInvariant(),
                Email = request.Email.Trim().ToLowerInvariant(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _userRepository.CreateAsync(user);
            return MapToDto(created);
        }

        public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            ValidateName(request.FirstName, nameof(request.FirstName));
            ValidateName(request.LastName, nameof(request.LastName));
            ValidateEmail(request.Email);

            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                throw new InvalidOperationException($"User with ID {id} not found.");

            var emailOwner = await _userRepository.GetByEmailAsync(request.Email);
            if (emailOwner is not null && emailOwner.Id != id)
                throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");

            user.Email = request.Email.Trim().ToLowerInvariant();
            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.Role = request.Role;
            user.UpdatedAt = DateTime.UtcNow;

            var updated = await _userRepository.UpdateAsync(user);
            return MapToDto(updated);
        }

        public async Task<UserDto> ActivateAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                throw new InvalidOperationException($"User with ID {id} not found.");
            if (user.Status == UserStatus.Active)
                throw new InvalidOperationException($"User with ID {id} is already active.");

            user.Status = UserStatus.Active;
            user.UpdatedAt = DateTime.UtcNow;
            var updated = await _userRepository.UpdateAsync(user);
            return MapToDto(updated);
        }

        public async Task<UserDto> DeactivateAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user is null)
                throw new InvalidOperationException($"User with ID {id} not found.");
            if (user.Status == UserStatus.Inactive)
                throw new InvalidOperationException($"User with ID {id} is already inactive.");

            user.Status = UserStatus.Inactive;
            user.UpdatedAt = DateTime.UtcNow;
            var updated = await _userRepository.UpdateAsync(user);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
            => await _userRepository.DeleteAsync(id);

        public async Task<LoginResponse> AuthenticateAsync(LoginRequest request)
        {
            if (request is null) throw new ArgumentNullException(nameof(request));

            // Anonymous access
            if (string.Equals(request.Username, "anonymous", StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrEmpty(request.Password))
            {
                return new LoginResponse
                {
                    Success = true,
                    IsAnonymous = true,
                    User = new UserDto
                    {
                        Id = Guid.Empty,
                        Username = "anonymous",
                        Email = "anonymous@system",
                        FirstName = "Anonymous",
                        LastName = "User",
                        Role = UserRole.Operator,
                        Status = UserStatus.Active
                    }
                };
            }

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return new LoginResponse { Success = false, ErrorMessage = "Username and password are required." };

            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user is null)
                return new LoginResponse { Success = false, ErrorMessage = "Invalid credentials." };

            if (user.Status == UserStatus.Locked)
                return new LoginResponse { Success = false, ErrorMessage = "Account is locked." };

            if (user.Status == UserStatus.Inactive)
                return new LoginResponse { Success = false, ErrorMessage = "Account is inactive." };

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return new LoginResponse { Success = false, ErrorMessage = "Invalid credentials." };

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new LoginResponse
            {
                Success = true,
                IsAnonymous = false,
                User = MapToDto(user)
            };
        }

        private static void ValidateName(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{paramName} cannot be empty.", paramName);
        }

        private static void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty.", nameof(username));
            if (username.Length < 3)
                throw new ArgumentException("Username must be at least 3 characters.", nameof(username));
        }

        private static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));

            try { _ = new MailAddress(email); }
            catch (FormatException)
            {
                throw new ArgumentException($"'{email}' is not a valid email address.", nameof(email));
            }
        }

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));
            if (password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters.", nameof(password));
        }

        private static UserDto MapToDto(User user) => new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Status = user.Status,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
