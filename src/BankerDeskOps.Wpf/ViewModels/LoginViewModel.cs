using BankerDeskOps.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly GrpcUserApiService _userApiService;
        private readonly SessionContext _sessionContext;
        private readonly ILogger<LoginViewModel> _logger;

        [ObservableProperty] private string username = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private bool isAnonymous;
        [ObservableProperty] private string? errorMessage;
        [ObservableProperty] private bool isLoading;

        public event Action? LoginSucceeded;

        public LoginViewModel(GrpcUserApiService userApiService, SessionContext sessionContext, ILogger<LoginViewModel> logger)
        {
            _userApiService = userApiService ?? throw new ArgumentNullException(nameof(userApiService));
            _sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [RelayCommand]
        public async Task Login()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                if (IsAnonymous)
                {
                    _logger.LogInformation("Anonymous login");
                    _sessionContext.CurrentUser = new Application.DTOs.UserDto
                    {
                        Id = Guid.Empty,
                        Username = "anonymous",
                        Email = "anonymous@system",
                        FirstName = "Anonymous",
                        LastName = "User",
                        Role = Domain.Enums.UserRole.Operator,
                        Status = Domain.Enums.UserStatus.Active
                    };
                    _sessionContext.IsAnonymous = true;
                    LoginSucceeded?.Invoke();
                    return;
                }

                if (string.IsNullOrWhiteSpace(Username))
                {
                    ErrorMessage = "Username is required";
                    return;
                }

                _logger.LogInformation("Login attempt for {Username}", Username);
                var response = await _userApiService.LoginAsync(Username, Password);

                if (response.Success)
                {
                    _sessionContext.CurrentUser = response.User;
                    _sessionContext.IsAnonymous = response.IsAnonymous;
                    LoginSucceeded?.Invoke();
                }
                else
                {
                    ErrorMessage = response.ErrorMessage ?? "Login failed";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Connection error: {ex.Message}";
                _logger.LogError("Login error: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnIsAnonymousChanged(bool value)
        {
            if (value)
            {
                Username = string.Empty;
                Password = string.Empty;
            }
        }
    }
}
