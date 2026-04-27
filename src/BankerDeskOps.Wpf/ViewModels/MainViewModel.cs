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
        private readonly BankClientsViewModel _bankClientsViewModel;
        private readonly UsersViewModel _usersViewModel;
        private readonly TransactionsViewModel _transactionsViewModel;

        [ObservableProperty]
        private ObservableObject? currentView;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            LoansViewModel loansViewModel,
            RetailAccountsViewModel retailAccountsViewModel,
            BankClientsViewModel bankClientsViewModel,
            UsersViewModel usersViewModel,
            TransactionsViewModel transactionsViewModel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loansViewModel = loansViewModel ?? throw new ArgumentNullException(nameof(loansViewModel));
            _retailAccountsViewModel = retailAccountsViewModel ?? throw new ArgumentNullException(nameof(retailAccountsViewModel));
            _bankClientsViewModel = bankClientsViewModel ?? throw new ArgumentNullException(nameof(bankClientsViewModel));
            _usersViewModel = usersViewModel ?? throw new ArgumentNullException(nameof(usersViewModel));
            _transactionsViewModel = transactionsViewModel ?? throw new ArgumentNullException(nameof(transactionsViewModel));
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

        /// <summary>
        /// Navigates to the Bank Clients view.
        /// </summary>
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

        /// <summary>
        /// Navigates to the Transactions view.
        /// </summary>
        [RelayCommand]
        public void NavigateToTransactions()
        {
            _logger.LogInformation("Navigating to Transactions view");
            CurrentView = _transactionsViewModel;
            _transactionsViewModel.LoadTransactionsCommand.Execute(null);
        }
    }
}
