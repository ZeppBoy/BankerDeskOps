using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Domain.Enums;
using BankerDeskOps.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace BankerDeskOps.Wpf.ViewModels
{
    /// <summary>
    /// ViewModel for managing retail accounts.
    /// </summary>
    public partial class RetailAccountsViewModel : ObservableObject
    {
        private readonly RetailAccountApiService _accountApiService;
        private readonly ILogger<RetailAccountsViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<RetailAccountDto> accounts = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private string customerName = string.Empty;

        [ObservableProperty]
        private AccountType accountType = AccountType.Checking;

        [ObservableProperty]
        private decimal initialDeposit;

        [ObservableProperty]
        private decimal transactionAmount;

        [ObservableProperty]
        private RetailAccountDto? selectedAccount;

        public RetailAccountsViewModel(RetailAccountApiService accountApiService, ILogger<RetailAccountsViewModel> logger)
        {
            _accountApiService = accountApiService ?? throw new ArgumentNullException(nameof(accountApiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loads all accounts from the API.
        /// </summary>
        [RelayCommand]
        public async Task LoadAccounts()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Loading accounts");

                var accounts = await _accountApiService.GetAllAccountsAsync();
                Accounts.Clear();
                foreach (var account in accounts)
                {
                    Accounts.Add(account);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading accounts: {ex.Message}";
                _logger.LogError("Failed to load accounts: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Opens a new account.
        /// </summary>
        [RelayCommand]
        public async Task OpenAccount()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CustomerName))
                {
                    ErrorMessage = "Customer name is required";
                    return;
                }

                IsLoading = true;
                ErrorMessage = null;

                var request = new CreateRetailAccountRequest
                {
                    CustomerName = CustomerName,
                    AccountType = AccountType,
                    InitialDeposit = InitialDeposit
                };

                _logger.LogInformation("Opening account for {CustomerName}", CustomerName);
                var createdAccount = await _accountApiService.OpenAccountAsync(request);

                if (createdAccount != null)
                {
                    Accounts.Add(createdAccount);
                    ClearForm();
                    ErrorMessage = "Account opened successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error opening account: {ex.Message}";
                _logger.LogError("Failed to open account: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Deposits funds into the selected account.
        /// </summary>
        [RelayCommand]
        public async Task Deposit()
        {
            if (SelectedAccount == null)
            {
                ErrorMessage = "Please select an account";
                return;
            }

            if (TransactionAmount <= 0)
            {
                ErrorMessage = "Deposit amount must be greater than zero";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var request = new DepositRequest { Amount = TransactionAmount };
                _logger.LogInformation("Depositing {Amount} to account {AccountId}", TransactionAmount, SelectedAccount.Id);

                var updatedAccount = await _accountApiService.DepositAsync(SelectedAccount.Id, request);
                if (updatedAccount != null)
                {
                    var index = Accounts.IndexOf(SelectedAccount);
                    if (index >= 0)
                    {
                        Accounts[index] = updatedAccount;
                        SelectedAccount = updatedAccount;
                    }
                    TransactionAmount = 0;
                    ErrorMessage = "Deposit successful";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error depositing: {ex.Message}";
                _logger.LogError("Failed to deposit: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Withdraws funds from the selected account.
        /// </summary>
        [RelayCommand]
        public async Task Withdraw()
        {
            if (SelectedAccount == null)
            {
                ErrorMessage = "Please select an account";
                return;
            }

            if (TransactionAmount <= 0)
            {
                ErrorMessage = "Withdrawal amount must be greater than zero";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var request = new WithdrawRequest { Amount = TransactionAmount };
                _logger.LogInformation("Withdrawing {Amount} from account {AccountId}", TransactionAmount, SelectedAccount.Id);

                var updatedAccount = await _accountApiService.WithdrawAsync(SelectedAccount.Id, request);
                if (updatedAccount != null)
                {
                    var index = Accounts.IndexOf(SelectedAccount);
                    if (index >= 0)
                    {
                        Accounts[index] = updatedAccount;
                        SelectedAccount = updatedAccount;
                    }
                    TransactionAmount = 0;
                    ErrorMessage = "Withdrawal successful";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error withdrawing: {ex.Message}";
                _logger.LogError("Failed to withdraw: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Closes the selected account.
        /// </summary>
        [RelayCommand]
        public async Task CloseAccount()
        {
            if (SelectedAccount == null)
            {
                ErrorMessage = "Please select an account to close";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Closing account {AccountId}", SelectedAccount.Id);

                await _accountApiService.CloseAccountAsync(SelectedAccount.Id);
                Accounts.Remove(SelectedAccount);
                SelectedAccount = null;
                ErrorMessage = "Account closed successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error closing account: {ex.Message}";
                _logger.LogError("Failed to close account: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearForm()
        {
            CustomerName = string.Empty;
            AccountType = AccountType.Checking;
            InitialDeposit = 0;
        }
    }
}
