using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Avalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace BankerDeskOps.Avalonia.ViewModels;

public partial class CurrencyViewModel : ObservableObject
{
    private readonly CurrencyApiService _currencyApiService;
    private readonly ILogger<CurrencyViewModel> _logger;

    [ObservableProperty] private ObservableCollection<CurrencyDto> currencies = new();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string code = string.Empty;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private CurrencyDto? selectedCurrency;
    [ObservableProperty] private Guid? editingId;

    public CurrencyViewModel(CurrencyApiService currencyApiService, ILogger<CurrencyViewModel> logger)
    {
        _currencyApiService = currencyApiService ?? throw new ArgumentNullException(nameof(currencyApiService));
        _logger             = logger             ?? throw new ArgumentNullException(nameof(logger));
    }

    [RelayCommand]
    public async Task LoadCurrencies()
    {
        try
        {
            IsLoading    = true;
            ErrorMessage = null;
            _logger.LogInformation("Loading currencies");

            var result = await _currencyApiService.GetAllCurrenciesAsync();
            Currencies.Clear();
            foreach (var currency in result)
                Currencies.Add(currency);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading currencies: {ex.Message}";
            _logger.LogError("Failed to load currencies: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task SaveCurrency()
    {
        if (!ValidateCode()) return;
        if (!ValidateName()) return;

        try
        {
            IsLoading    = true;
            ErrorMessage = null;

            if (EditingId.HasValue)
            {
                var request = new UpdateCurrencyRequest
                {
                    Id   = EditingId.Value,
                    Code = Code,
                    Name = Name
                };

                _logger.LogInformation("Updating currency {CurrencyId}", EditingId.Value);
                var updated = await _currencyApiService.UpdateCurrencyAsync(request);

                if (updated != null)
                {
                    ReplaceInList(updated);
                    ErrorMessage = "Currency updated successfully";
                }
            }
            else
            {
                var request = new CreateCurrencyRequest
                {
                    Code = Code,
                    Name = Name
                };

                _logger.LogInformation("Creating currency {Code}", Code);
                var created = await _currencyApiService.CreateCurrencyAsync(request);

                if (created != null)
                {
                    Currencies.Add(created);
                    ErrorMessage = "Currency created successfully";
                }
            }

            ClearForm();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error saving currency: {ex.Message}";
            _logger.LogError("Failed to save currency: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void EditCurrency()
    {
        if (SelectedCurrency == null)
        {
            ErrorMessage = "Please select a currency to edit";
            return;
        }

        EditingId  = SelectedCurrency.Id;
        Code       = SelectedCurrency.Code;
        Name       = SelectedCurrency.Name;
        ErrorMessage = null;
    }

    [RelayCommand]
    public async Task DeleteCurrency()
    {
        if (SelectedCurrency == null)
        {
            ErrorMessage = "Please select a currency to delete";
            return;
        }

        try
        {
            IsLoading    = true;
            ErrorMessage = null;
            _logger.LogInformation("Deleting currency {CurrencyId}", SelectedCurrency.Id);

            var deletingId = SelectedCurrency.Id;
            await _currencyApiService.DeleteCurrencyAsync(deletingId);
            Currencies.Remove(SelectedCurrency);
            ErrorMessage     = "Currency deleted successfully";

            if (EditingId == deletingId)
                ClearForm();
            else
                SelectedCurrency = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting currency: {ex.Message}";
            _logger.LogError("Failed to delete currency: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void CancelEdit()
    {
        ClearForm();
        ErrorMessage = null;
    }

    private bool ValidateCode()
    {
        if (string.IsNullOrWhiteSpace(Code))
        {
            ErrorMessage = "Currency code is required";
            return false;
        }

        if (Code.Length != 3)
        {
            ErrorMessage = "Currency code must be exactly 3 characters";
            return false;
        }

        if (HasDuplicateCode(Code))
        {
            ErrorMessage = $"A currency with code '{Code}' already exists";
            return false;
        }

        return true;
    }

    private bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Currency name is required";
            return false;
        }

        return true;
    }

    private bool HasDuplicateCode(string code)
    {
        return Currencies.Any(c => c.Code.ToUpperInvariant() == code.ToUpperInvariant()
            && (!EditingId.HasValue || c.Id != EditingId.Value));
    }

    private void ReplaceInList(CurrencyDto updated)
    {
        var index = Currencies.IndexOf(SelectedCurrency!);
        if (index >= 0)
        {
            Currencies[index] = updated;
            SelectedCurrency  = updated;
        }
    }

    private void ClearForm()
    {
        EditingId      = null;
        Code           = string.Empty;
        Name           = string.Empty;
        SelectedCurrency = null;
    }
}
