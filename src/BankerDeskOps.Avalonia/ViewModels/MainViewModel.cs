using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Avalonia.ViewModels;

/// <summary>
/// Main ViewModel — manages top-level navigation between feature views.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ILogger<MainViewModel> _logger;
    private readonly LoansViewModel _loansViewModel;
    private readonly RetailAccountsViewModel _retailAccountsViewModel;
    private readonly BankClientsViewModel _bankClientsViewModel;

    [ObservableProperty]
    private ObservableObject? currentView;

    public MainViewModel(
        ILogger<MainViewModel> logger,
        LoansViewModel loansViewModel,
        RetailAccountsViewModel retailAccountsViewModel,
        BankClientsViewModel bankClientsViewModel)
    {
        _logger                  = logger                  ?? throw new ArgumentNullException(nameof(logger));
        _loansViewModel          = loansViewModel          ?? throw new ArgumentNullException(nameof(loansViewModel));
        _retailAccountsViewModel = retailAccountsViewModel ?? throw new ArgumentNullException(nameof(retailAccountsViewModel));
        _bankClientsViewModel    = bankClientsViewModel    ?? throw new ArgumentNullException(nameof(bankClientsViewModel));
    }

    [RelayCommand]
    public void NavigateToLoans()
    {
        _logger.LogInformation("Navigating to Loans view");
        CurrentView = _loansViewModel;
        _loansViewModel.LoadLoansCommand.Execute(null);
    }

    [RelayCommand]
    public void NavigateToRetailAccounts()
    {
        _logger.LogInformation("Navigating to Retail Accounts view");
        CurrentView = _retailAccountsViewModel;
        _retailAccountsViewModel.LoadAccountsCommand.Execute(null);
    }

    [RelayCommand]
    public void NavigateToBankClients()
    {
        _logger.LogInformation("Navigating to Bank Clients view");
        CurrentView = _bankClientsViewModel;
        _bankClientsViewModel.LoadClientsCommand.Execute(null);
    }
}
