using System.Windows.Threading;
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
        private readonly Dispatcher? _dispatcher;

        [ObservableProperty] private string username = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private bool isAnonymous;
        [ObservableProperty] private string errorMessage = string.Empty;
        [ObservableProperty] private bool isLoading;

        public bool IsErrorMessageVisible => !string.IsNullOrEmpty(ErrorMessage);

        partial void OnErrorMessageChanged(string value)
        {
            OnPropertyChanged(nameof(IsErrorMessageVisible));
        }

        public event Action? LoginSucceeded;

        public LoginViewModel(GrpcUserApiService userApiService, SessionContext sessionContext, ILogger<LoginViewModel> logger)
        {
            _userApiService = userApiService ?? throw new ArgumentNullException(nameof(userApiService));
            _sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dispatcher = Dispatcher.CurrentDispatcher;
        }

        [RelayCommand]
        public async Task Login()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

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
                    IsLoading = false;
                    LoginSucceeded?.Invoke();
                    return;
                }

                if (string.IsNullOrWhiteSpace(Username))
                {
                    ErrorMessage = "Username is required";
                    IsLoading = false;
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
                _logger.LogError(ex, "Login error");
                ErrorMessage = $"Connection error: {ex.Message}";
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
