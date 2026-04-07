using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace BankerDeskOps.Wpf.ViewModels
{
    /// <summary>
    /// ViewModel for managing loans.
    /// </summary>
    public partial class LoansViewModel : ObservableObject
    {
        private readonly GrpcLoanApiService _loanApiService;
        private readonly ILogger<LoansViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<LoanDto> loans = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private string customerName = string.Empty;

        [ObservableProperty]
        private decimal amount;

        [ObservableProperty]
        private decimal interestRate;

        [ObservableProperty]
        private int termMonths;

        [ObservableProperty]
        private LoanDto? selectedLoan;

        public LoansViewModel(GrpcLoanApiService loanApiService, ILogger<LoansViewModel> logger)
        {
            _loanApiService = loanApiService ?? throw new ArgumentNullException(nameof(loanApiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loads all loans from the API.
        /// </summary>
        [RelayCommand]
        public async Task LoadLoans()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Loading loans");

                var loans = await _loanApiService.GetAllLoansAsync();
                Loans.Clear();
                foreach (var loan in loans)
                {
                    Loans.Add(loan);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading loans: {ex.Message}";
                _logger.LogError("Failed to load loans: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Creates a new loan.
        /// </summary>
        [RelayCommand]
        public async Task CreateLoan()
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

                var request = new CreateLoanRequest
                {
                    CustomerName = CustomerName,
                    Amount = Amount,
                    InterestRate = InterestRate,
                    TermMonths = TermMonths
                };

                _logger.LogInformation("Creating loan for {CustomerName}", CustomerName);
                var createdLoan = await _loanApiService.CreateLoanAsync(request);

                if (createdLoan != null)
                {
                    Loans.Add(createdLoan);
                    ClearForm();
                    ErrorMessage = "Loan created successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating loan: {ex.Message}";
                _logger.LogError("Failed to create loan: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Approves the selected loan.
        /// </summary>
        [RelayCommand]
        public async Task ApproveLoan()
        {
            if (SelectedLoan == null)
            {
                ErrorMessage = "Please select a loan to approve";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Approving loan {LoanId}", SelectedLoan.Id);

                var updatedLoan = await _loanApiService.ApproveLoanAsync(SelectedLoan.Id);
                if (updatedLoan != null)
                {
                    var index = Loans.IndexOf(SelectedLoan);
                    if (index >= 0)
                    {
                        Loans[index] = updatedLoan;
                        SelectedLoan = updatedLoan;
                    }
                    ErrorMessage = "Loan approved successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error approving loan: {ex.Message}";
                _logger.LogError("Failed to approve loan: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Rejects the selected loan.
        /// </summary>
        [RelayCommand]
        public async Task RejectLoan()
        {
            if (SelectedLoan == null)
            {
                ErrorMessage = "Please select a loan to reject";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Rejecting loan {LoanId}", SelectedLoan.Id);

                var updatedLoan = await _loanApiService.RejectLoanAsync(SelectedLoan.Id);
                if (updatedLoan != null)
                {
                    var index = Loans.IndexOf(SelectedLoan);
                    if (index >= 0)
                    {
                        Loans[index] = updatedLoan;
                        SelectedLoan = updatedLoan;
                    }
                    ErrorMessage = "Loan rejected successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error rejecting loan: {ex.Message}";
                _logger.LogError("Failed to reject loan: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Deletes the selected loan.
        /// </summary>
        [RelayCommand]
        public async Task DeleteLoan()
        {
            if (SelectedLoan == null)
            {
                ErrorMessage = "Please select a loan to delete";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Deleting loan {LoanId}", SelectedLoan.Id);

                await _loanApiService.DeleteLoanAsync(SelectedLoan.Id);
                Loans.Remove(SelectedLoan);
                SelectedLoan = null;
                ErrorMessage = "Loan deleted successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting loan: {ex.Message}";
                _logger.LogError("Failed to delete loan: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearForm()
        {
            CustomerName = string.Empty;
            Amount = 0;
            InterestRate = 0;
            TermMonths = 0;
        }
    }
}
