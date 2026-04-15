using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Avalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace BankerDeskOps.Avalonia.ViewModels;

public partial class LoansViewModel : ObservableObject
{
    private readonly GrpcLoanApiService _loanApiService;
    private readonly ILogger<LoansViewModel> _logger;

    [ObservableProperty] private ObservableCollection<LoanDto> loans = new();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string customerName = string.Empty;
    [ObservableProperty] private decimal amount;
    [ObservableProperty] private decimal interestRate;
    [ObservableProperty] private int termMonths;
    [ObservableProperty] private LoanDto? selectedLoan;

    public LoansViewModel(GrpcLoanApiService loanApiService, ILogger<LoansViewModel> logger)
    {
        _loanApiService = loanApiService ?? throw new ArgumentNullException(nameof(loanApiService));
        _logger         = logger         ?? throw new ArgumentNullException(nameof(logger));
    }

    [RelayCommand]
    public async Task LoadLoans()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = null;
            _logger.LogInformation("Loading loans");

            var result = await _loanApiService.GetAllLoansAsync();
            Loans.Clear();
            foreach (var loan in result)
                Loans.Add(loan);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading loans: {ex.Message}";
            _logger.LogError("Failed to load loans: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task CreateLoan()
    {
        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            ErrorMessage = "Customer name is required";
            return;
        }

        try
        {
            IsLoading    = true;
            ErrorMessage = null;

            var request = new CreateLoanRequest
            {
                CustomerName = CustomerName,
                Amount       = Amount,
                InterestRate = InterestRate,
                TermMonths   = TermMonths
            };

            _logger.LogInformation("Creating loan for {CustomerName}", CustomerName);
            var created = await _loanApiService.CreateLoanAsync(request);

            if (created != null)
            {
                Loans.Add(created);
                ClearForm();
                ErrorMessage = "Loan created successfully";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error creating loan: {ex.Message}";
            _logger.LogError("Failed to create loan: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task ApproveLoan()
    {
        if (SelectedLoan == null) { ErrorMessage = "Please select a loan to approve"; return; }

        try
        {
            IsLoading    = true;
            ErrorMessage = null;
            _logger.LogInformation("Approving loan {LoanId}", SelectedLoan.Id);

            var updated = await _loanApiService.ApproveLoanAsync(SelectedLoan.Id);
            if (updated != null) ReplaceInList(updated, ref updated);

            ErrorMessage = "Loan approved successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error approving loan: {ex.Message}";
            _logger.LogError("Failed to approve loan: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task RejectLoan()
    {
        if (SelectedLoan == null) { ErrorMessage = "Please select a loan to reject"; return; }

        try
        {
            IsLoading    = true;
            ErrorMessage = null;
            _logger.LogInformation("Rejecting loan {LoanId}", SelectedLoan.Id);

            var updated = await _loanApiService.RejectLoanAsync(SelectedLoan.Id);
            if (updated != null) ReplaceInList(updated, ref updated);

            ErrorMessage = "Loan rejected successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error rejecting loan: {ex.Message}";
            _logger.LogError("Failed to reject loan: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task DisburseLoan()
    {
        if (SelectedLoan == null)
        {
            ErrorMessage = "Please select an approved loan to disburse";
            return;
        }

        if (SelectedLoan.Status != Domain.Enums.LoanStatus.Approved)
        {
            ErrorMessage = $"Only Approved loans can be disbursed. Current status: {SelectedLoan.Status}";
            return;
        }

        try
        {
            IsLoading    = true;
            ErrorMessage = null;
            _logger.LogInformation("Disbursing loan {LoanId}", SelectedLoan.Id);

            var updated = await _loanApiService.DisburseLoanAsync(SelectedLoan.Id);
            if (updated != null) ReplaceInList(updated, ref updated);

            ErrorMessage = "Loan disbursed successfully. Contract has been created automatically.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error disbursing loan: {ex.Message}";
            _logger.LogError("Failed to disburse loan {LoanId}: {Message}", SelectedLoan?.Id, ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task DeleteLoan()
    {
        if (SelectedLoan == null) { ErrorMessage = "Please select a loan to delete"; return; }

        try
        {
            IsLoading    = true;
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
        finally { IsLoading = false; }
    }

    private void ReplaceInList(LoanDto updated, ref LoanDto? selected)
    {
        var index = Loans.IndexOf(SelectedLoan!);
        if (index >= 0)
        {
            Loans[index] = updated;
            SelectedLoan = updated;
        }
    }

    private void ClearForm()
    {
        CustomerName = string.Empty;
        Amount       = 0;
        InterestRate = 0;
        TermMonths   = 0;
    }
}
