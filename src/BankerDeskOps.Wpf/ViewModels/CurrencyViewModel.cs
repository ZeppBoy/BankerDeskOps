using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace BankerDeskOps.Wpf.ViewModels
{
    public partial class CurrencyViewModel : ObservableObject, INotifyDataErrorInfo
    {
        private readonly CurrencyApiService _currencyApiService;
        private readonly ILogger<CurrencyViewModel> _logger;
        private Dictionary<string, List<string>> _errors = new();

        [ObservableProperty]
        private ObservableCollection<CurrencyDto> currencies = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private string code = string.Empty;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private CurrencyDto? selectedCurrency;

        [ObservableProperty]
        private Guid? editingId;

        public bool HasErrors => _errors.Values.Any(e => e.Count > 0);
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public CurrencyViewModel(CurrencyApiService currencyApiService, ILogger<CurrencyViewModel> logger)
        {
            _currencyApiService = currencyApiService ?? throw new ArgumentNullException(nameof(currencyApiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [RelayCommand]
        public async Task LoadCurrencies()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Loading currencies");

                var currencies = await _currencyApiService.GetAllCurrenciesAsync();
                Currencies.Clear();
                foreach (var currency in currencies)
                {
                    Currencies.Add(currency);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading currencies: {ex.Message}";
                _logger.LogError("Failed to load currencies: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SaveCurrency()
        {
            var validationErrors = ValidateCurrency();
            if (validationErrors.Count > 0)
            {
                ErrorMessage = string.Join("; ", validationErrors.SelectMany(e => e.Value));
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                if (EditingId.HasValue)
                {
                    var request = new UpdateCurrencyRequest
                    {
                        Id = EditingId.Value,
                        Code = Code,
                        Name = Name
                    };

                    _logger.LogInformation("Updating currency {CurrencyId}", EditingId.Value);
                    var updated = await _currencyApiService.UpdateCurrencyAsync(request);

                    if (updated != null)
                    {
                        var index = Currencies.IndexOf(SelectedCurrency!);
                        if (index >= 0)
                            Currencies[index] = updated;
                        SelectedCurrency = updated;
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
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void EditCurrency()
        {
            if (SelectedCurrency == null)
            {
                ErrorMessage = "Please select a currency to edit";
                return;
            }

            EditingId = SelectedCurrency.Id;
            Code = SelectedCurrency.Code;
            Name = SelectedCurrency.Name;
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

            var result = MessageBox.Show(
                $"Are you sure you want to delete the currency \"{SelectedCurrency.Code}\" ({SelectedCurrency.Name})?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Deleting currency {CurrencyId}", SelectedCurrency.Id);

                await _currencyApiService.DeleteCurrencyAsync(SelectedCurrency.Id);
                Currencies.Remove(SelectedCurrency);
                SelectedCurrency = null;
                ClearForm();
                ErrorMessage = "Currency deleted successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting currency: {ex.Message}";
                _logger.LogError("Failed to delete currency: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void CancelEdit()
        {
            ClearForm();
            ErrorMessage = null;
        }

        private Dictionary<string, List<string>> ValidateCurrency()
        {
            var errors = new Dictionary<string, List<string>>();

            if (string.IsNullOrWhiteSpace(Code))
                errors["Code"] = new List<string> { "Currency code is required." };
            else if (Code.Length < 3 || Code.Length > 3)
                errors["Code"] = new List<string> { "Currency code must be exactly 3 characters." };

            if (string.IsNullOrWhiteSpace(Name))
                errors["Name"] = new List<string> { "Currency name is required." };

            if (!errors.Any())
            {
                var existing = Currencies.FirstOrDefault(c => c.Code.Equals(Code, StringComparison.OrdinalIgnoreCase) && c.Id != (EditingId ?? Guid.Empty));
                if (existing != null)
                    errors["Code"] = new List<string> { "A currency with this code already exists." };
            }

            _errors = errors;
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(string.Empty));
            return errors;
        }

        public System.Collections.IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName == null || !_errors.TryGetValue(propertyName, out var errors))
                return Enumerable.Empty<string>();
            return errors;
        }

        private void ClearForm()
        {
            EditingId = null;
            Code = string.Empty;
            Name = string.Empty;
            SelectedCurrency = null;
        }
    }
}
