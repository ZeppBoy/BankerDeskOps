using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.ViewModels
{
    /// <summary>
    /// Main ViewModel for the application window.
    /// Handles navigation between different views.
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly ILogger<MainViewModel> _logger;
        private readonly LoansViewModel _loansViewModel;
        private readonly RetailAccountsViewModel _retailAccountsViewModel;

        [ObservableProperty]
        private ObservableObject? currentView;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            LoansViewModel loansViewModel,
            RetailAccountsViewModel retailAccountsViewModel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loansViewModel = loansViewModel ?? throw new ArgumentNullException(nameof(loansViewModel));
            _retailAccountsViewModel = retailAccountsViewModel ?? throw new ArgumentNullException(nameof(retailAccountsViewModel));
        }

        /// <summary>
        /// Navigates to the Loans view.
        /// </summary>
        [RelayCommand]
        public void NavigateToLoans()
        {
            _logger.LogInformation("Navigating to Loans view");
            CurrentView = _loansViewModel;
            _loansViewModel.LoadLoansCommand.Execute(null);
        }

        /// <summary>
        /// Navigates to the Retail Accounts view.
        /// </summary>
        [RelayCommand]
        public void NavigateToRetailAccounts()
        {
            _logger.LogInformation("Navigating to Retail Accounts view");
            CurrentView = _retailAccountsViewModel;
            _retailAccountsViewModel.LoadAccountsCommand.Execute(null);
        }
    }
}
