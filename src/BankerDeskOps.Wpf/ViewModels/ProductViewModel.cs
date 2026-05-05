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
    public partial class ProductViewModel : ObservableObject, INotifyDataErrorInfo
    {
        private readonly ProductApiService _productApiService;
        private readonly CurrencyApiService _currencyApiService;
        private readonly ILogger<ProductViewModel> _logger;
        private Dictionary<string, List<string>> _errors = new();

        [ObservableProperty]
        private ObservableCollection<ProductDto> products = new();

        [ObservableProperty]
        private ObservableCollection<CurrencyDto> currencies = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private string name = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        [ObservableProperty]
        private bool isActive = true;

        [ObservableProperty]
        private int term = 12;

        [ObservableProperty]
        private decimal minAmount = 0m;

        [ObservableProperty]
        private decimal maxAmount = 0m;

        [ObservableProperty]
        private Guid currencyId = Guid.Empty;

        [ObservableProperty]
        private ProductDto? selectedProduct;

        [ObservableProperty]
        private Guid? editingId;

        public bool HasErrors => _errors.Values.Any(e => e.Count > 0);
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public ProductViewModel(ProductApiService productApiService, CurrencyApiService currencyApiService, ILogger<ProductViewModel> logger)
        {
            _productApiService = productApiService ?? throw new ArgumentNullException(nameof(productApiService));
            _currencyApiService = currencyApiService ?? throw new ArgumentNullException(nameof(currencyApiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [RelayCommand]
        public async Task LoadProducts()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Loading products");

                var products = await _productApiService.GetAllProductsAsync();
                Products.Clear();
                foreach (var product in products)
                {
                    Products.Add(product);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading products: {ex.Message}";
                _logger.LogError("Failed to load products: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadCurrencies()
        {
            try
            {
                _logger.LogInformation("Loading currencies");

                var currenciesList = await _currencyApiService.GetAllCurrenciesAsync();
                Currencies.Clear();
                foreach (var currency in currenciesList)
                {
                    Currencies.Add(currency);
                }

                // Auto-select first currency if none selected
                if (CurrencyId == Guid.Empty && Currencies.Count > 0)
                {
                    CurrencyId = Currencies[0].Id;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading currencies: {ex.Message}";
                _logger.LogError("Failed to load currencies: {Message}", ex.Message);
            }
        }

        [RelayCommand]
        public async Task SaveProduct()
        {
            var validationErrors = ValidateProduct();
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
                    var request = new UpdateProductRequest
                    {
                        Id = EditingId.Value,
                        Name = Name,
                        Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                        IsActive = IsActive,
                        Term = Term,
                        MinAmount = MinAmount,
                        MaxAmount = MaxAmount,
                        CurrencyId = CurrencyId
                    };

                    _logger.LogInformation("Updating product {ProductId}", EditingId.Value);
                    var updated = await _productApiService.UpdateProductAsync(request);

                    if (updated != null)
                    {
                        var index = Products.IndexOf(SelectedProduct!);
                        if (index >= 0)
                            Products[index] = updated;
                        SelectedProduct = updated;
                        ErrorMessage = "Product updated successfully";
                    }
                }
                else
                {
                    var request = new CreateProductRequest
                    {
                        Name = Name,
                        Description = string.IsNullOrWhiteSpace(Description) ? null : Description,
                        IsActive = IsActive,
                        Term = Term,
                        MinAmount = MinAmount,
                        MaxAmount = MaxAmount,
                        CurrencyId = CurrencyId
                    };

                    _logger.LogInformation("Creating product {Name}", Name);
                    var created = await _productApiService.CreateProductAsync(request);

                    if (created != null)
                    {
                        Products.Add(created);
                        ErrorMessage = "Product created successfully";
                    }
                }

                ClearForm();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error saving product: {ex.Message}";
                _logger.LogError("Failed to save product: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void EditProduct()
        {
            if (SelectedProduct == null)
            {
                ErrorMessage = "Please select a product to edit";
                return;
            }

            EditingId = SelectedProduct.Id;
            Name = SelectedProduct.Name;
            Description = SelectedProduct.Description ?? string.Empty;
            IsActive = SelectedProduct.IsActive;
            Term = SelectedProduct.Term;
            MinAmount = SelectedProduct.MinAmount;
            MaxAmount = SelectedProduct.MaxAmount;
            CurrencyId = SelectedProduct.CurrencyId;
            ErrorMessage = null;
        }

        [RelayCommand]
        public async Task DeleteProduct()
        {
            if (SelectedProduct == null)
            {
                ErrorMessage = "Please select a product to delete";
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete the product \"{SelectedProduct.Name}\"?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Deleting product {ProductId}", SelectedProduct.Id);

                await _productApiService.DeleteProductAsync(SelectedProduct.Id);
                Products.Remove(SelectedProduct);
                SelectedProduct = null;
                ClearForm();
                ErrorMessage = "Product deleted successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting product: {ex.Message}";
                _logger.LogError("Failed to delete product: {Message}", ex.Message);
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

        private Dictionary<string, List<string>> ValidateProduct()
        {
            var errors = new Dictionary<string, List<string>>();

            if (string.IsNullOrWhiteSpace(Name))
                errors["Name"] = new List<string> { "Product name is required." };

            if (Term <= 0)
                errors["Term"] = new List<string> { "Term must be greater than 0." };

            if (MinAmount < 0)
                errors["MinAmount"] = new List<string> { "Minimum amount cannot be negative." };

            if (MaxAmount <= 0)
                errors["MaxAmount"] = new List<string> { "Maximum amount must be greater than 0." };

            if (CurrencyId == Guid.Empty)
                errors["CurrencyId"] = new List<string> { "Please select a currency." };

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
            Name = string.Empty;
            Description = string.Empty;
            IsActive = true;
            Term = 12;
            MinAmount = 0m;
            MaxAmount = 0m;
            CurrencyId = Guid.Empty;
            SelectedProduct = null;
        }
    }
}
