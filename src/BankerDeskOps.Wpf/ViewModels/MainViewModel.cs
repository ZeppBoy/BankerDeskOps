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
        private readonly CurrencyViewModel _currencyViewModel;
        private readonly ProductViewModel _productViewModel;
        private readonly RateViewModel _rateViewModel;
        private readonly FeeViewModel _feeViewModel;
                private readonly CommissionViewModel _commissionViewModel;
        private readonly LoanApplicationReviewViewModel _loanApplicationReviewViewModel;
        private readonly RepaymentScheduleViewModel _repaymentScheduleViewModel;

        [ObservableProperty]
        private ObservableObject? currentView;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            LoansViewModel loansViewModel,
            RetailAccountsViewModel retailAccountsViewModel,
            BankClientsViewModel bankClientsViewModel,
            UsersViewModel usersViewModel,
            TransactionsViewModel transactionsViewModel,
            CurrencyViewModel currencyViewModel,
            ProductViewModel productViewModel,
            RateViewModel rateViewModel,
            FeeViewModel feeViewModel,
                        CommissionViewModel commissionViewModel,
            LoanApplicationReviewViewModel loanApplicationReviewViewModel,
            RepaymentScheduleViewModel repaymentScheduleViewModel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loansViewModel = loansViewModel ?? throw new ArgumentNullException(nameof(loansViewModel));
            _retailAccountsViewModel = retailAccountsViewModel ?? throw new ArgumentNullException(nameof(retailAccountsViewModel));
            _bankClientsViewModel = bankClientsViewModel ?? throw new ArgumentNullException(nameof(bankClientsViewModel));
            _usersViewModel = usersViewModel ?? throw new ArgumentNullException(nameof(usersViewModel));
            _transactionsViewModel = transactionsViewModel ?? throw new ArgumentNullException(nameof(transactionsViewModel));
            _currencyViewModel = currencyViewModel ?? throw new ArgumentNullException(nameof(currencyViewModel));
            _productViewModel = productViewModel ?? throw new ArgumentNullException(nameof(productViewModel));
            _rateViewModel = rateViewModel ?? throw new ArgumentNullException(nameof(rateViewModel));
            _feeViewModel = feeViewModel ?? throw new ArgumentNullException(nameof(feeViewModel));
                        _commissionViewModel = commissionViewModel ?? throw new ArgumentNullException(nameof(commissionViewModel));
            _loanApplicationReviewViewModel = loanApplicationReviewViewModel ?? throw new ArgumentNullException(nameof(loanApplicationReviewViewModel));
            _repaymentScheduleViewModel = repaymentScheduleViewModel ?? throw new ArgumentNullException(nameof(repaymentScheduleViewModel));
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

        [RelayCommand]
        public void NavigateToCurrencies()
        {
            _logger.LogInformation("Navigating to Currency Management view");
            CurrentView = _currencyViewModel;
            _currencyViewModel.LoadCurrenciesCommand.Execute(null);
        }

        [RelayCommand]
        public void NavigateToProducts()
        {
            _logger.LogInformation("Navigating to Product Management view");
            CurrentView = _productViewModel;
            _productViewModel.LoadProductsCommand.Execute(null);
        }

        [RelayCommand]
        public void NavigateToRates()
        {
            _logger.LogInformation("Navigating to Rate Configuration view");
            CurrentView = _rateViewModel;
            _rateViewModel.LoadRatesCommand.Execute(null);
        }

        [RelayCommand]
        public void NavigateToFees()
        {
            _logger.LogInformation("Navigating to Fee Management view");
            CurrentView = _feeViewModel;
            _feeViewModel.LoadFeesCommand.Execute(null);
        }

        [RelayCommand]
        public void NavigateToCommissions()
        {
            _logger.LogInformation("Navigating to Commission Management view");
            CurrentView = _commissionViewModel;
            _commissionViewModel.LoadCommissionsCommand.Execute(null);
        }

                [RelayCommand]
        public void NavigateToLoanApplicationReview()
        {
            _logger.LogInformation("Navigating to Loan Application Review view");
            CurrentView = _loanApplicationReviewViewModel;
            _loanApplicationReviewViewModel.LoadApplicationsCommand.Execute(null);
        }

        [RelayCommand]
        public void NavigateToRepaymentSchedules()
        {
            _logger.LogInformation("Navigating to Repayment Schedules view");
            CurrentView = _repaymentScheduleViewModel;
            _repaymentScheduleViewModel.LoadAllCommand.Execute(null);
        }
    }
}
