using System.Collections.ObjectModel;
using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.ViewModels
{
    /// <summary>
    /// ViewModel for managing transactions and fund transfers.
    /// </summary>
    public partial class TransactionsViewModel : ObservableObject
    {
        private readonly GrpcTransactionApiService _grpcClient;
        private readonly GrpcRetailAccountApiService _accountClient;
        private readonly ILogger<TransactionsViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<TransactionDto> _transactions = new();

        [ObservableProperty]
        private TransactionDto? _selectedTransaction;

        [ObservableProperty]
        private ObservableCollection<RetailAccountDto> _sourceAccounts = new();

        [ObservableProperty]
        private ObservableCollection<RetailAccountDto> _destinationAccounts = new();

        [ObservableProperty]
        private Guid _sourceAccountId = Guid.Empty;

        [ObservableProperty]
        private Guid _destinationAccountId = Guid.Empty;

        [ObservableProperty]
        private decimal _transferAmount;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string? _errorMessage;

        public TransactionsViewModel(GrpcTransactionApiService grpcClient, GrpcRetailAccountApiService accountClient, ILogger<TransactionsViewModel> logger)
        {
            _grpcClient = grpcClient ?? throw new ArgumentNullException(nameof(grpcClient));
            _accountClient = accountClient ?? throw new ArgumentNullException(nameof(accountClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loads all retail accounts for the source and destination dropdowns.
        /// </summary>
        private async Task LoadAccountsAsync()
        {
            try
            {
                _logger.LogInformation("Loading retail accounts for transfer dropdowns");
                var accounts = await _accountClient.GetAllAccountsAsync();
                SourceAccounts.Clear();
                DestinationAccounts.Clear();
                foreach (var account in accounts)
                {
                    SourceAccounts.Add(account);
                    DestinationAccounts.Add(account);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading retail accounts for dropdowns");
            }
        }

        /// <summary>
        /// Loads all transactions from the API.
        /// </summary>
        [RelayCommand]
        private async Task LoadTransactionsAsync()
        {
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                await LoadAccountsAsync();

                _logger.LogInformation("Loading transactions");
                var result = await _grpcClient.GetAllTransactionsAsync();
                Transactions.Clear();
                foreach (var transaction in result)
                {
                    Transactions.Add(transaction);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load transactions: {ex.Message}";
                _logger.LogError(ex, "Error loading transactions");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Executes a fund transfer between two accounts.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanTransfer))]
        private async Task TransferFundsAsync()
        {
            ErrorMessage = null;
            try
            {
                _logger.LogInformation("Transferring funds from {SourceAccountId} to {DestinationAccountId}", SourceAccountId, DestinationAccountId);

                var request = new TransferRequest
                {
                    FromAccountId = SourceAccountId,
                    ToAccountId = DestinationAccountId,
                    Amount = TransferAmount,
                    Description = string.IsNullOrWhiteSpace(Description) ? null : Description
                };

                await _grpcClient.TransferFundsAsync(request);

                _logger.LogInformation("Transfer successful");
                ErrorMessage = null;

                await LoadTransactionsAsync();
                ClearForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Transfer failed: {ex.Message}";
                _logger.LogError(ex, "Error executing transfer");
            }
        }

        /// <summary>
        /// Clears the transfer form fields.
        /// </summary>
        [RelayCommand]
        private void ClearForm()
        {
            SourceAccountId = Guid.Empty;
            DestinationAccountId = Guid.Empty;
            TransferAmount = 0m;
            Description = string.Empty;
        }

        /// <summary>
        /// Determines if a transfer can be executed.
        /// </summary>
        private bool CanTransfer()
        {
            return SourceAccountId != Guid.Empty
                && DestinationAccountId != Guid.Empty
                && SourceAccountId != DestinationAccountId
                && TransferAmount > 0;
        }

        /// <summary>
        /// Notifies that the transfer command can execute state may have changed.
        /// </summary>
        partial void OnSourceAccountIdChanged(Guid value)
        {
            ((IAsyncRelayCommand)TransferFundsCommand).NotifyCanExecuteChanged();
        }

        partial void OnDestinationAccountIdChanged(Guid value)
        {
            ((IAsyncRelayCommand)TransferFundsCommand).NotifyCanExecuteChanged();
        }

        partial void OnTransferAmountChanged(decimal value)
        {
            ((IAsyncRelayCommand)TransferFundsCommand).NotifyCanExecuteChanged();
        }
    }
}
