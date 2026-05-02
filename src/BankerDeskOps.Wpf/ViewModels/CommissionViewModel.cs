using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BankerDeskOps.Wpf.ViewModels
{
    public partial class CommissionViewModel : ObservableObject, INotifyDataErrorInfo
    {
        private readonly CommissionApiService _commissionApiService;
        private readonly ILogger<CommissionViewModel> _logger;
        private Dictionary<string, List<string>> _errors = new();

        [ObservableProperty]
        private ObservableCollection<CommissionDto> commissions = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private Guid productId;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private decimal percentage;

        [ObservableProperty]
        private CommissionDto? selectedCommission;

        [ObservableProperty]
        private Guid? editingId;

        public bool HasErrors => _errors.Values.Any(e => e.Count > 0);
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public CommissionViewModel(CommissionApiService commissionApiService, ILogger<CommissionViewModel> logger)
        {
            _commissionApiService = commissionApiService ?? throw new ArgumentNullException(nameof(commissionApiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [RelayCommand]
        public async Task LoadCommissions()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Loading commissions");

                var commissions = await _commissionApiService.GetAllCommissionsAsync();
                Commissions.Clear();
                foreach (var commission in commissions)
                {
                    Commissions.Add(commission);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading commissions: {ex.Message}";
                _logger.LogError("Failed to load commissions: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadCommissionsByProduct()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Loading commissions for product {ProductId}", ProductId);

                var commissions = await _commissionApiService.GetCommissionsByProductIdAsync(ProductId);
                Commissions.Clear();
                foreach (var commission in commissions)
                {
                    Commissions.Add(commission);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading commissions: {ex.Message}";
                _logger.LogError("Failed to load commissions for product: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SaveCommission()
        {
            var validationErrors = ValidateCommission();
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
                    var request = new UpdateCommissionRequest
                    {
                        Id = EditingId.Value,
                        Name = Name,
                        Percentage = Percentage
                    };

                    _logger.LogInformation("Updating commission {CommissionId}", EditingId.Value);
                    var updated = await _commissionApiService.UpdateCommissionAsync(request);

                    if (updated != null)
                    {
                        var index = Commissions.IndexOf(SelectedCommission!);
                        if (index >= 0)
                            Commissions[index] = updated;
                        SelectedCommission = updated;
                        ErrorMessage = "Commission updated successfully";
                    }
                }
                else
                {
                    var request = new CreateCommissionRequest
                    {
                        ProductId = ProductId,
                        Name = Name,
                        Percentage = Percentage
                    };

                    _logger.LogInformation("Creating commission {Name}", Name);
                    var created = await _commissionApiService.CreateCommissionAsync(request);

                    if (created != null)
                    {
                        Commissions.Add(created);
                        ErrorMessage = "Commission created successfully";
                    }
                }

                ClearForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving commission: {ex.Message}";
                _logger.LogError("Failed to save commission: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void EditCommission()
        {
            if (SelectedCommission == null)
            {
                ErrorMessage = "Please select a commission to edit";
                return;
            }

            EditingId = SelectedCommission.Id;
            ProductId = SelectedCommission.ProductId;
            Name = SelectedCommission.Name;
            Percentage = SelectedCommission.Percentage;
            ErrorMessage = null;
        }

        [RelayCommand]
        public async Task DeleteCommission()
        {
            if (SelectedCommission == null)
            {
                ErrorMessage = "Please select a commission to delete";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Deleting commission {CommissionId}", SelectedCommission.Id);

                await _commissionApiService.DeleteCommissionAsync(SelectedCommission.Id);
                Commissions.Remove(SelectedCommission);
                SelectedCommission = null;
                ClearForm();
                ErrorMessage = "Commission deleted successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting commission: {ex.Message}";
                _logger.LogError("Failed to delete commission: {Message}", ex.Message);
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

        private Dictionary<string, List<string>> ValidateCommission()
        {
            var errors = new Dictionary<string, List<string>>();

            if (string.IsNullOrWhiteSpace(Name))
                errors["Name"] = new List<string> { "Commission name is required." };

            if (Percentage < 0)
                errors["Percentage"] = new List<string> { "Percentage cannot be negative." };

            if (Percentage > 100)
                errors["Percentage"] = new List<string> { "Percentage cannot exceed 100%." };

            _errors = errors;
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(string.Empty));
            return errors;
        }

        public IEnumerable<string>? GetErrors(string? propertyName)
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
            Percentage = 0;
            SelectedCommission = null;
        }
    }
}
