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
    public partial class FeeViewModel : ObservableObject, INotifyDataErrorInfo
    {
        private readonly FeeApiService _feeApiService;
        private readonly ILogger<FeeViewModel> _logger;
        private Dictionary<string, List<string>> _errors = new();

        [ObservableProperty]
        private ObservableCollection<FeeDto> fees = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private Guid productId;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private decimal amount;

        [ObservableProperty]
        private FeeDto? selectedFee;

        [ObservableProperty]
        private Guid? editingId;

        public bool HasErrors => _errors.Values.Any(e => e.Count > 0);
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

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

                var fees = await _feeApiService.GetAllFeesAsync();
                Fees.Clear();
                foreach (var fee in fees)
                {
                    Fees.Add(fee);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading fees: {ex.Message}";
                _logger.LogError("Failed to load fees: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadFeesByProduct()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Loading fees for product {ProductId}", ProductId);

                var fees = await _feeApiService.GetFeesByProductIdAsync(ProductId);
                Fees.Clear();
                foreach (var fee in fees)
                {
                    Fees.Add(fee);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading fees: {ex.Message}";
                _logger.LogError("Failed to load fees for product: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SaveFee()
        {
            var validationErrors = ValidateFee();
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
                    var request = new UpdateFeeRequest
                    {
                        Id = EditingId.Value,
                        Name = Name,
                        Amount = Amount
                    };

                    _logger.LogInformation("Updating fee {FeeId}", EditingId.Value);
                    var updated = await _feeApiService.UpdateFeeAsync(request);

                    if (updated != null)
                    {
                        var index = Fees.IndexOf(SelectedFee!);
                        if (index >= 0)
                            Fees[index] = updated;
                        SelectedFee = updated;
                        ErrorMessage = "Fee updated successfully";
                    }
                }
                else
                {
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
                        ErrorMessage = "Fee created successfully";
                    }
                }

                ClearForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving fee: {ex.Message}";
                _logger.LogError("Failed to save fee: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void EditFee()
        {
            if (SelectedFee == null)
            {
                ErrorMessage = "Please select a fee to edit";
                return;
            }

            EditingId = SelectedFee.Id;
            ProductId = SelectedFee.ProductId;
            Name = SelectedFee.Name;
            Amount = SelectedFee.Amount;
            ErrorMessage = null;
        }

        [RelayCommand]
        public async Task DeleteFee()
        {
            if (SelectedFee == null)
            {
                ErrorMessage = "Please select a fee to delete";
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete the fee \"{SelectedFee.Name}\"?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Deleting fee {FeeId}", SelectedFee.Id);

                await _feeApiService.DeleteFeeAsync(SelectedFee.Id);
                Fees.Remove(SelectedFee);
                SelectedFee = null;
                ClearForm();
                ErrorMessage = "Fee deleted successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting fee: {ex.Message}";
                _logger.LogError("Failed to delete fee: {Message}", ex.Message);
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

        private Dictionary<string, List<string>> ValidateFee()
        {
            var errors = new Dictionary<string, List<string>>();

            if (string.IsNullOrWhiteSpace(Name))
                errors["Name"] = new List<string> { "Fee name is required." };

            if (Amount < 0)
                errors["Amount"] = new List<string> { "Fee amount cannot be negative." };

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
            Name = string.Empty;
            Amount = 0;
            SelectedFee = null;
        }
    }
}
