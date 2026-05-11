using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Avalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace BankerDeskOps.Avalonia.ViewModels;

public partial class RateViewModel : ObservableObject
{
    private readonly RateApiService _rateApiService;
    private readonly ILogger<RateViewModel> _logger;

    [ObservableProperty] private ObservableCollection<RateDto> rates = new();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private Guid productId;
    [ObservableProperty] private decimal minAmount;
    [ObservableProperty] private decimal maxAmount;
    [ObservableProperty] private int minTermMonths = 1;
    [ObservableProperty] private int maxTermMonths = 12;
    [ObservableProperty] private decimal rateValue;
    [ObservableProperty] private RateDto? selectedRate;
    [ObservableProperty] private Guid? editingId;

    public RateViewModel(RateApiService rateApiService, ILogger<RateViewModel> logger)
    {
        _rateApiService = rateApiService ?? throw new ArgumentNullException(nameof(rateApiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [RelayCommand]
    public async Task LoadRates()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            _logger.LogInformation("Loading all rates");

            var result = await _rateApiService.GetAllRatesAsync();
            Rates.Clear();
            foreach (var rate in result)
                Rates.Add(rate);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading rates: {ex.Message}";
            _logger.LogError("Failed to load rates: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task LoadRatesByProduct()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            _logger.LogInformation("Loading rates for product {ProductId}", ProductId);

            var result = await _rateApiService.GetRatesByProductIdAsync(ProductId);
            Rates.Clear();
            foreach (var rate in result)
                Rates.Add(rate);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading rates by product: {ex.Message}";
            _logger.LogError("Failed to load rates by product: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task SaveRate()
    {
        if (!ValidateRateData()) return;

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            if (EditingId.HasValue)
            {
                _logger.LogInformation("Updating rate {RateId}", EditingId.Value);
                var request = new UpdateRateRequest
                {
                    Id = EditingId.Value,
                    MinAmount = MinAmount,
                    MaxAmount = MaxAmount,
                    MinTermMonths = MinTermMonths,
                    MaxTermMonths = MaxTermMonths,
                    RateValue = RateValue
                };

                var updated = await _rateApiService.UpdateRateAsync(request);
                if (updated != null)
                {
                    ReplaceInList(updated);
                    ErrorMessage = "Rate updated successfully";
                }
            }
            else
            {
                _logger.LogInformation("Creating new rate for product {ProductId}", ProductId);
                var request = new CreateRateRequest
                {
                    ProductId = ProductId,
                    MinAmount = MinAmount,
                    MaxAmount = MaxAmount,
                    MinTermMonths = MinTermMonths,
                    MaxTermMonths = MaxTermMonths,
                    RateValue = RateValue
                };

                var created = await _rateApiService.CreateRateAsync(request);
                if (created != null)
                {
                    Rates.Add(created);
                    ErrorMessage = "Rate created successfully";
                }
            }

            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving rate: {ex.Message}";
            _logger.LogError("Failed to save rate: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void EditRate()
    {
        if (SelectedRate == null)
        {
            ErrorMessage = "Please select a rate to edit";
            return;
        }

        EditingId = SelectedRate.Id;
        ProductId = SelectedRate.ProductId;
        MinAmount = SelectedRate.MinAmount;
        MaxAmount = SelectedRate.MaxAmount;
        MinTermMonths = SelectedRate.MinTermMonths;
        MaxTermMonths = SelectedRate.MaxTermMonths;
        RateValue = SelectedRate.RateValue;

        ErrorMessage = null;
    }

    [RelayCommand]
    public async Task DeleteRate()
    {
        if (SelectedRate == null)
        {
            ErrorMessage = "Please select a rate to delete";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            _logger.LogInformation("Deleting rate {RateId}", SelectedRate.Id);

            await _rateApiService.DeleteRateAsync(SelectedRate.Id);
            Rates.Remove(SelectedRate);
            SelectedRate = null;
            ErrorMessage = "Rate deleted successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting rate: {ex.Message}";
            _logger.LogError("Failed to delete rate: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void CancelEdit()
    {
        ClearForm();
        ErrorMessage = null;
    }

    private bool ValidateRateData()
    {
        if (MinAmount < 0)
        {
            ErrorMessage = "Min Amount must be >= 0";
            return false;
        }

        if (MaxAmount <= 0)
        {
            ErrorMessage = "Max Amount must be > 0";
            return false;
        }

        if (MaxAmount <= MinAmount)
        {
            ErrorMessage = "Max Amount must be greater than Min Amount";
            return false;
        }

        if (MinTermMonths < 1)
        {
            ErrorMessage = "Min Term Months must be >= 1";
            return false;
        }

        if (MaxTermMonths < 1)
        {
            ErrorMessage = "Max Term Months must be >= 1";
            return false;
        }

        if (MaxTermMonths < MinTermMonths)
        {
            ErrorMessage = "Max Term Months must be >= Min Term Months";
            return false;
        }

        if (RateValue < 0 || RateValue > 100)
        {
            ErrorMessage = "Rate Value must be between 0 and 100";
            return false;
        }

        return true;
    }

    private void ReplaceInList(RateDto updated)
    {
        var index = Rates.IndexOf(SelectedRate!);
        if (index >= 0)
        {
            Rates[index] = updated;
            SelectedRate = updated;
        }
    }

    private void ClearForm()
    {
        EditingId = null;
        ProductId = Guid.Empty;
        MinAmount = 0;
        MaxAmount = 0;
        MinTermMonths = 1;
        MaxTermMonths = 12;
        RateValue = 0;
    }
}
