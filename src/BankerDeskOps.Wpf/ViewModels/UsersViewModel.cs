using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Domain.Enums;
using BankerDeskOps.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace BankerDeskOps.Wpf.ViewModels
{
    public partial class UsersViewModel : ObservableObject
    {
        private readonly GrpcUserApiService _userApiService;
        private readonly ILogger<UsersViewModel> _logger;

        [ObservableProperty] private ObservableCollection<UserDto> users = new();
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private string? errorMessage;
        [ObservableProperty] private UserDto? selectedUser;

        // Form fields
        [ObservableProperty] private string username = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string firstName = string.Empty;
        [ObservableProperty] private string lastName = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private UserRole selectedRole = UserRole.Operator;

        public Array RoleValues => Enum.GetValues(typeof(UserRole));

        public UsersViewModel(GrpcUserApiService userApiService, ILogger<UsersViewModel> logger)
        {
            _userApiService = userApiService ?? throw new ArgumentNullException(nameof(userApiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [RelayCommand]
        public async Task LoadUsers()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Loading users");

                var result = await _userApiService.GetAllUsersAsync();
                Users.Clear();
                foreach (var u in result)
                    Users.Add(u);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading users: {ex.Message}";
                _logger.LogError("Failed to load users: {Message}", ex.Message);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        public async Task CreateUser()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(FirstName)
                || string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Username, first name, last name and email are required";
                return;
            }

            if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8)
            {
                ErrorMessage = "Password must be at least 8 characters";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var request = new CreateUserRequest
                {
                    Username = Username,
                    Email = Email,
                    FirstName = FirstName,
                    LastName = LastName,
                    Password = Password,
                    Role = SelectedRole
                };

                _logger.LogInformation("Creating user {Username}", Username);
                var created = await _userApiService.CreateUserAsync(request);
                if (created != null)
                {
                    Users.Add(created);
                    ClearForm();
                    ErrorMessage = "User created successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating user: {ex.Message}";
                _logger.LogError("Failed to create user: {Message}", ex.Message);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        public async Task UpdateUser()
        {
            if (SelectedUser == null) { ErrorMessage = "Please select a user to update"; return; }

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var request = new UpdateUserRequest
                {
                    Email = Email,
                    FirstName = FirstName,
                    LastName = LastName,
                    Role = SelectedRole
                };

                _logger.LogInformation("Updating user {UserId}", SelectedUser.Id);
                var updated = await _userApiService.UpdateUserAsync(SelectedUser.Id, request);
                if (updated != null)
                {
                    var index = Users.IndexOf(SelectedUser);
                    if (index >= 0)
                    {
                        Users[index] = updated;
                        SelectedUser = updated;
                    }
                    ErrorMessage = "User updated successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating user: {ex.Message}";
                _logger.LogError("Failed to update user: {Message}", ex.Message);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        public async Task ActivateUser()
        {
            if (SelectedUser == null) { ErrorMessage = "Please select a user"; return; }
            if (SelectedUser.Status == UserStatus.Active) { ErrorMessage = "User is already active"; return; }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                var updated = await _userApiService.ActivateUserAsync(SelectedUser.Id);
                if (updated != null)
                {
                    var index = Users.IndexOf(SelectedUser);
                    if (index >= 0) { Users[index] = updated; SelectedUser = updated; }
                    ErrorMessage = "User activated successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error activating user: {ex.Message}";
                _logger.LogError("Failed to activate user: {Message}", ex.Message);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        public async Task DeactivateUser()
        {
            if (SelectedUser == null) { ErrorMessage = "Please select a user"; return; }
            if (SelectedUser.Status == UserStatus.Inactive) { ErrorMessage = "User is already inactive"; return; }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                var updated = await _userApiService.DeactivateUserAsync(SelectedUser.Id);
                if (updated != null)
                {
                    var index = Users.IndexOf(SelectedUser);
                    if (index >= 0) { Users[index] = updated; SelectedUser = updated; }
                    ErrorMessage = "User deactivated successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deactivating user: {ex.Message}";
                _logger.LogError("Failed to deactivate user: {Message}", ex.Message);
            }
            finally { IsLoading = false; }
        }

        [RelayCommand]
        public async Task DeleteUser()
        {
            if (SelectedUser == null) { ErrorMessage = "Please select a user"; return; }

            if (System.Windows.MessageBox.Show($"Delete user '{SelectedUser.Username}'?",
                "Confirm Delete", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning)
                != System.Windows.MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                var deleted = await _userApiService.DeleteUserAsync(SelectedUser.Id);
                if (deleted)
                {
                    Users.Remove(SelectedUser);
                    SelectedUser = null;
                    ClearForm();
                    ErrorMessage = "User deleted successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting user: {ex.Message}";
                _logger.LogError("Failed to delete user: {Message}", ex.Message);
            }
            finally { IsLoading = false; }
        }

        partial void OnSelectedUserChanged(UserDto? value)
        {
            if (value is null) return;
            Username = value.Username;
            Email = value.Email;
            FirstName = value.FirstName;
            LastName = value.LastName;
            SelectedRole = value.Role;
            Password = string.Empty;
        }

        private void ClearForm()
        {
            Username = string.Empty;
            Email = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            Password = string.Empty;
            SelectedRole = UserRole.Operator;
        }
    }
}
