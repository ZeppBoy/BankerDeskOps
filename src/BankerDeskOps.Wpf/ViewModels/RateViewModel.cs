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
    public partial class RateViewModel : ObservableObject, INotifyDataErrorInfo
    {
        private readonly RateApiService _rateApiService;
        private readonly ILogger<RateViewModel> _logger;
        private Dictionary<string, List<string>> _errors = new();

        [ObservableProperty]
        private ObservableCollection<RateDto> rates = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private Guid productId;

        [ObservableProperty]
        private decimal minAmount;

        [ObservableProperty]
        private decimal maxAmount;

        [ObservableProperty]
        private int minTermMonths = 1;

        [ObservableProperty]
        private int maxTermMonths = 12;

        [ObservableProperty]
        private decimal rateValue;

        [ObservableProperty]
        private RateDto? selectedRate;

        [ObservableProperty]
        private Guid? editingId;

        public bool HasErrors => _errors.Values.Any(e => e.Count > 0);
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

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
                _logger.LogInformation("Loading rates");

                var rates = await _rateApiService.GetAllRatesAsync();
                Rates.Clear();
                foreach (var rate in rates)
                {
                    Rates.Add(rate);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading rates: {ex.Message}";
                _logger.LogError("Failed to load rates: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadRatesByProduct()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Loading rates for product {ProductId}", ProductId);

                var rates = await _rateApiService.GetRatesByProductIdAsync(ProductId);
                Rates.Clear();
                foreach (var rate in rates)
                {
                    Rates.Add(rate);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading rates: {ex.Message}";
                _logger.LogError("Failed to load rates for product: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SaveRate()
        {
            var validationErrors = ValidateRate();
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
                    var request = new UpdateRateRequest
                    {
                        Id = EditingId.Value,
                        MinAmount = MinAmount,
                        MaxAmount = MaxAmount,
                        MinTermMonths = MinTermMonths,
                        MaxTermMonths = MaxTermMonths,
                        RateValue = RateValue
                    };

                    _logger.LogInformation("Updating rate {RateId}", EditingId.Value);
                    var updated = await _rateApiService.UpdateRateAsync(request);

                    if (updated != null)
                    {
                        var index = Rates.IndexOf(SelectedRate!);
                        if (index >= 0)
                            Rates[index] = updated;
                        SelectedRate = updated;
                        ErrorMessage = "Rate updated successfully";
                    }
                }
                else
                {
                    var request = new CreateRateRequest
                    {
                        ProductId = ProductId,
                        MinAmount = MinAmount,
                        MaxAmount = MaxAmount,
                        MinTermMonths = MinTermMonths,
                        MaxTermMonths = MaxTermMonths,
                        RateValue = RateValue
                    };

                    _logger.LogInformation("Creating rate");
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
            finally
            {
                IsLoading = false;
            }
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

            var result = MessageBox.Show(
                $"Are you sure you want to delete the rate ({SelectedRate.RateValue}%) for product {SelectedRate.ProductId}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Deleting rate {RateId}", SelectedRate.Id);

                await _rateApiService.DeleteRateAsync(SelectedRate.Id);
                Rates.Remove(SelectedRate);
                SelectedRate = null;
                ClearForm();
                ErrorMessage = "Rate deleted successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting rate: {ex.Message}";
                _logger.LogError("Failed to delete rate: {Message}", ex.Message);
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

        private Dictionary<string, List<string>> ValidateRate()
        {
            var errors = new Dictionary<string, List<string>>();

            if (MinAmount < 0)
                errors["MinAmount"] = new List<string> { "Minimum amount cannot be negative." };

            if (MaxAmount <= 0)
                errors["MaxAmount"] = new List<string> { "Maximum amount must be positive." };

            if (MinAmount > MaxAmount && MinAmount >= 0 && MaxAmount > 0)
                errors["MaxAmount"] = new List<string> { "Maximum amount must be greater than minimum amount." };

            if (MinTermMonths < 1)
                errors["MinTermMonths"] = new List<string> { "Minimum term must be at least 1 month." };

            if (MaxTermMonths < 1)
                errors["MaxTermMonths"] = new List<string> { "Maximum term must be at least 1 month." };

            if (MinTermMonths > MaxTermMonths)
                errors["MaxTermMonths"] = new List<string> { "Maximum term must be greater than or equal to minimum term." };

            if (RateValue < 0)
                errors["RateValue"] = new List<string> { "Rate value cannot be negative." };

            if (RateValue > 100)
                errors["RateValue"] = new List<string> { "Rate value cannot exceed 100%." };

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
            ProductId = Guid.Empty;
            MinAmount = 0;
            MaxAmount = 0;
            MinTermMonths = 1;
            MaxTermMonths = 12;
            RateValue = 0;
            SelectedRate = null;
        }
    }
}
