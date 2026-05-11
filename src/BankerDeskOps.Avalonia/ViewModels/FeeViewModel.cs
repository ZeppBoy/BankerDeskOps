using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Avalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace BankerDeskOps.Avalonia.ViewModels;

public partial class FeeViewModel : ObservableObject
{
    private readonly FeeApiService _feeApiService;
    private readonly ILogger<FeeViewModel> _logger;

    [ObservableProperty] private ObservableCollection<FeeDto> fees = new();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private Guid productId;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private decimal amount;
    [ObservableProperty] private FeeDto? selectedFee;
    [ObservableProperty] private Guid? editingId;

    public FeeViewModel(FeeApiService feeApiService, ILogger<FeeViewModel> logger)
    {
        _feeApiService = feeApiService ?? throw new ArgumentNullException(nameof(feeApiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [RelayCommand]
    public async Task LoadFees()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            _logger.LogInformation("Loading fees");

            var result = await _feeApiService.GetAllFeesAsync();
            Fees.Clear();
            foreach (var fee in result)
                Fees.Add(fee);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading fees: {ex.Message}";
            _logger.LogError("Failed to load fees: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task LoadFeesByProduct()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            _logger.LogInformation("Loading fees for product {ProductId}", ProductId);

            var result = await _feeApiService.GetFeesByProductIdAsync(ProductId);
            Fees.Clear();
            foreach (var fee in result)
                Fees.Add(fee);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading fees by product: {ex.Message}";
            _logger.LogError("Failed to load fees by product: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task SaveFee()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Fee name is required";
            return;
        }

        if (Amount < 0)
        {
            ErrorMessage = "Amount must be greater than or equal to 0";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var request = new CreateFeeRequest
            {
                ProductId = ProductId,
                Name = Name,
                Amount = Amount
            };

            _logger.LogInformation("Creating fee {Name}", Name);
            var created = await _feeApiService.CreateFeeAsync(request);

            if (created != null)
            {
                Fees.Add(created);
                ClearForm();
                ErrorMessage = "Fee saved successfully";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving fee: {ex.Message}";
            _logger.LogError("Failed to save fee: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task EditFee()
    {
        if (SelectedFee == null)
        {
            ErrorMessage = "Please select a fee to edit";
            return;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Fee name is required";
            return;
        }

        if (Amount < 0)
        {
            ErrorMessage = "Amount must be greater than or equal to 0";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            _logger.LogInformation("Updating fee {FeeId}", SelectedFee.Id);

            var request = new UpdateFeeRequest
            {
                Id = SelectedFee.Id,
                Name = Name,
                Amount = Amount
            };

            var updated = await _feeApiService.UpdateFeeAsync(request);
            if (updated != null)
            {
                ReplaceInList(updated);
                ClearForm();
                ErrorMessage = "Fee updated successfully";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error editing fee: {ex.Message}";
            _logger.LogError("Failed to edit fee: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task DeleteFee()
    {
        if (SelectedFee == null)
        {
            ErrorMessage = "Please select a fee to delete";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            _logger.LogInformation("Deleting fee {FeeId}", SelectedFee.Id);

            await _feeApiService.DeleteFeeAsync(SelectedFee.Id);
            Fees.Remove(SelectedFee);
            SelectedFee = null;
            ErrorMessage = "Fee deleted successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting fee: {ex.Message}";
            _logger.LogError("Failed to delete fee: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void CancelEdit()
    {
        ClearForm();
        ErrorMessage = null;
    }

    private void ReplaceInList(FeeDto updated)
    {
        var index = Fees.IndexOf(SelectedFee!);
        if (index >= 0)
        {
            Fees[index] = updated;
            SelectedFee = updated;
        }
    }

    private void ClearForm()
    {
        ProductId = Guid.Empty;
        Name = string.Empty;
        Amount = 0;
        EditingId = null;
    }
}
