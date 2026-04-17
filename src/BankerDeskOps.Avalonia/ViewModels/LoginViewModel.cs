using BankerDeskOps.Avalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Avalonia.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly GrpcUserApiService _userApiService;
    private readonly SessionContext _sessionContext;
    private readonly ILogger<LoginViewModel> _logger;

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private bool isAnonymous;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;

    public event Action? LoginSucceeded;

    public string LoginButtonText => IsAnonymous ? "Enter as Guest" : "Log In";

    public LoginViewModel(
        GrpcUserApiService userApiService,
        SessionContext sessionContext,
        ILogger<LoginViewModel> logger)
    {
        _userApiService = userApiService ?? throw new ArgumentNullException(nameof(userApiService));
        _sessionContext = sessionContext ?? throw new ArgumentNullException(nameof(sessionContext));
        _logger         = logger         ?? throw new ArgumentNullException(nameof(logger));
    }

    partial void OnIsAnonymousChanged(bool value)
    {
        OnPropertyChanged(nameof(LoginButtonText));
        if (value)
        {
            Username = "anonymous";
            Password = string.Empty;
        }
        else
        {
            Username = string.Empty;
        }
    }

    [RelayCommand]
    public async Task Login()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = null;

            _logger.LogInformation("Login attempt for {Username}", Username);
            var response = await _userApiService.LoginAsync(
                IsAnonymous ? "anonymous" : Username,
                IsAnonymous ? string.Empty : Password);

            if (response.Success)
            {
                _sessionContext.CurrentUser = response.User;
                _sessionContext.IsAnonymous = response.IsAnonymous;
                _logger.LogInformation("Login succeeded for {Username}", Username);
                LoginSucceeded?.Invoke();
            }
            else
            {
                ErrorMessage = response.ErrorMessage ?? "Login failed";
                _logger.LogWarning("Login failed for {Username}: {Error}", Username, ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Connection error: {ex.Message}";
            _logger.LogError("Login error: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }
}
