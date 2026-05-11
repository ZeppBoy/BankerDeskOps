using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Avalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace BankerDeskOps.Avalonia.ViewModels;

public partial class ProductViewModel : ObservableObject
{
    private readonly ProductApiService _productApiService;
    private readonly CurrencyApiService _currencyApiService;
    private readonly ILogger<ProductViewModel> _logger;

    [ObservableProperty] private ObservableCollection<ProductDto> products = new();
    [ObservableProperty] private ObservableCollection<CurrencyDto> currencies = new();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private bool isActive = true;
    [ObservableProperty] private int term = 12;
    [ObservableProperty] private decimal minAmount;
    [ObservableProperty] private decimal maxAmount;
    [ObservableProperty] private Guid currencyId;
    [ObservableProperty] private ProductDto? selectedProduct;
    [ObservableProperty] private Guid? editingId;
    [ObservableProperty] private CurrencyDto? selectedCurrencyForProduct;

    partial void OnSelectedCurrencyForProductChanged(CurrencyDto? value)
    {
        if (value != null)
            CurrencyId = value.Id;
    }

    public ProductViewModel(ProductApiService productApiService, CurrencyApiService currencyApiService, ILogger<ProductViewModel> logger)
    {
        _productApiService = productApiService ?? throw new ArgumentNullException(nameof(productApiService));
        _currencyApiService = currencyApiService ?? throw new ArgumentNullException(nameof(currencyApiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _ = LoadCurrencies();
        _ = LoadProducts();
    }

    [RelayCommand]
    public async Task LoadProducts()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            _logger.LogInformation("Loading products");

            var result = await _productApiService.GetAllProductsAsync();
            Products.Clear();
            foreach (var product in result)
                Products.Add(product);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading products: {ex.Message}";
            _logger.LogError("Failed to load products: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    public async Task LoadCurrencies()
    {
        try
        {
            _logger.LogInformation("Loading currencies");

            var result = await _currencyApiService.GetAllCurrenciesAsync();
            Currencies.Clear();
            foreach (var currency in result)
                Currencies.Add(currency);

            if (CurrencyId == Guid.Empty && Currencies.Count > 0)
            {
                CurrencyId = Currencies[0].Id;
                SelectedCurrencyForProduct = Currencies[0];
            }
            else if (CurrencyId != Guid.Empty)
            {
                SelectedCurrencyForProduct = Currencies.FirstOrDefault(c => c.Id == CurrencyId);
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
        finally { IsLoading = false; }
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
        SelectedCurrencyForProduct = Currencies.FirstOrDefault(c => c.Id == SelectedProduct.CurrencyId);
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
        finally { IsLoading = false; }
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
