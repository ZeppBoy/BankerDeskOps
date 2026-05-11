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
    private readonly UsersViewModel _usersViewModel;
    private readonly CurrencyViewModel _currencyViewModel;
    private readonly ProductViewModel _productViewModel;
    private readonly RateViewModel _rateViewModel;
    private readonly FeeViewModel _feeViewModel;
    private readonly CommissionViewModel _commissionViewModel;

    [ObservableProperty]
    private ObservableObject? currentView;

    public MainViewModel(
        ILogger<MainViewModel> logger,
        LoansViewModel loansViewModel,
        RetailAccountsViewModel retailAccountsViewModel,
        BankClientsViewModel bankClientsViewModel,
        UsersViewModel usersViewModel,
        CurrencyViewModel currencyViewModel,
        ProductViewModel productViewModel,
        RateViewModel rateViewModel,
        FeeViewModel feeViewModel,
        CommissionViewModel commissionViewModel)
    {
        _logger                  = logger                  ?? throw new ArgumentNullException(nameof(logger));
        _loansViewModel          = loansViewModel          ?? throw new ArgumentNullException(nameof(loansViewModel));
        _retailAccountsViewModel = retailAccountsViewModel ?? throw new ArgumentNullException(nameof(retailAccountsViewModel));
        _bankClientsViewModel    = bankClientsViewModel    ?? throw new ArgumentNullException(nameof(bankClientsViewModel));
        _usersViewModel          = usersViewModel          ?? throw new ArgumentNullException(nameof(usersViewModel));
        _currencyViewModel       = currencyViewModel       ?? throw new ArgumentNullException(nameof(currencyViewModel));
        _productViewModel        = productViewModel        ?? throw new ArgumentNullException(nameof(productViewModel));
        _rateViewModel           = rateViewModel           ?? throw new ArgumentNullException(nameof(rateViewModel));
        _feeViewModel            = feeViewModel            ?? throw new ArgumentNullException(nameof(feeViewModel));
        _commissionViewModel     = commissionViewModel     ?? throw new ArgumentNullException(nameof(commissionViewModel));
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

    [RelayCommand]
    public void NavigateToUsers()
    {
        _logger.LogInformation("Navigating to Users view");
        CurrentView = _usersViewModel;
        _usersViewModel.LoadUsersCommand.Execute(null);
    }

    [RelayCommand]
    public void NavigateToCurrencies()
    {
        _logger.LogInformation("Navigating to Currencies view");
        CurrentView = _currencyViewModel;
        _currencyViewModel.LoadCurrenciesCommand.Execute(null);
    }

    [RelayCommand]
    public void NavigateToProducts()
    {
        _logger.LogInformation("Navigating to Products view");
        CurrentView = _productViewModel;
        _productViewModel.LoadProductsCommand.Execute(null);
    }

    [RelayCommand]
    public void NavigateToRates()
    {
        _logger.LogInformation("Navigating to Rates view");
        CurrentView = _rateViewModel;
        _rateViewModel.LoadRatesCommand.Execute(null);
    }

    [RelayCommand]
    public void NavigateToFees()
    {
        _logger.LogInformation("Navigating to Fees view");
        CurrentView = _feeViewModel;
        _feeViewModel.LoadFeesCommand.Execute(null);
    }

    [RelayCommand]
    public void NavigateToCommissions()
    {
        _logger.LogInformation("Navigating to Commissions view");
        CurrentView = _commissionViewModel;
        _commissionViewModel.LoadCommissionsCommand.Execute(null);
    }
}
