using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Avalonia.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace BankerDeskOps.Avalonia.ViewModels;

public partial class CommissionViewModel : ObservableObject
{
    private readonly CommissionApiService _commissionApiService;
    private readonly ILogger<CommissionViewModel> _logger;

    [ObservableProperty] private ObservableCollection<CommissionDto> commissions = new();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private Guid productId;
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private decimal percentage;
    [ObservableProperty] private CommissionDto? selectedCommission;
    [ObservableProperty] private Guid? editingId;

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

            var result = await _commissionApiService.GetAllCommissionsAsync();
            Commissions.Clear();
            foreach (var commission in result)
                Commissions.Add(commission);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading commissions: {ex.Message}";
            _logger.LogError("Failed to load commissions: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task LoadCommissionsByProduct()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            _logger.LogInformation("Loading commissions for product {ProductId}", ProductId);

            var result = await _commissionApiService.GetCommissionsByProductIdAsync(ProductId);
            Commissions.Clear();
            foreach (var commission in result)
                Commissions.Add(commission);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading commissions by product: {ex.Message}";
            _logger.LogError("Failed to load commissions by product: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public async Task SaveCommission()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Commission name is required";
            return;
        }

        if (Percentage < 0 || Percentage > 100)
        {
            ErrorMessage = "Percentage must be between 0 and 100";
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
                    ReplaceInList(updated);
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
        finally { IsLoading = false; }
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
            ErrorMessage = "Commission deleted successfully";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting commission: {ex.Message}";
            _logger.LogError("Failed to delete commission: {Message}", ex.Message);
        }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    public void CancelEdit()
    {
        ClearForm();
        ErrorMessage = null;
    }

    private void ReplaceInList(CommissionDto updated)
    {
        var index = Commissions.IndexOf(SelectedCommission!);
        if (index >= 0)
        {
            Commissions[index] = updated;
            SelectedCommission = updated;
        }
    }

    private void ClearForm()
    {
        EditingId = null;
        ProductId = Guid.Empty;
        Name = string.Empty;
        Percentage = 0;
    }
}
