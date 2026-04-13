using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Domain.Enums;
using BankerDeskOps.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace BankerDeskOps.Wpf.ViewModels
{
    public partial class BankClientsViewModel : ObservableObject
    {
        private readonly GrpcBankClientApiService _clientApiService;
        private readonly ILogger<BankClientsViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<BankClientDto> clients = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private BankClientDto? selectedClient;

        // Form fields — create / edit
        [ObservableProperty] private string firstName = string.Empty;
        [ObservableProperty] private string lastName = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string phoneNumber = string.Empty;
        [ObservableProperty] private DateTime dateOfBirth = new DateTime(1990, 1, 1);
        [ObservableProperty] private string nationalId = string.Empty;
        [ObservableProperty] private string street = string.Empty;
        [ObservableProperty] private string city = string.Empty;
        [ObservableProperty] private string postalCode = string.Empty;
        [ObservableProperty] private string country = string.Empty;

        public BankClientsViewModel(GrpcBankClientApiService clientApiService, ILogger<BankClientsViewModel> logger)
        {
            _clientApiService = clientApiService ?? throw new ArgumentNullException(nameof(clientApiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [RelayCommand]
        public async Task LoadClients()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Loading bank clients");

                var result = await _clientApiService.GetAllClientsAsync();
                Clients.Clear();
                foreach (var c in result)
                    Clients.Add(c);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading clients: {ex.Message}";
                _logger.LogError("Failed to load bank clients: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task CreateClient()
        {
            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
            {
                ErrorMessage = "First name and last name are required";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var request = new CreateBankClientRequest
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    DateOfBirth = DateOfBirth,
                    NationalId = NationalId,
                    Street = Street,
                    City = City,
                    PostalCode = PostalCode,
                    Country = Country
                };

                _logger.LogInformation("Creating bank client {Email}", Email);
                var created = await _clientApiService.CreateClientAsync(request);
                if (created != null)
                {
                    Clients.Add(created);
                    ClearForm();
                    ErrorMessage = "Client created successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating client: {ex.Message}";
                _logger.LogError("Failed to create bank client: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task UpdateClient()
        {
            if (SelectedClient == null)
            {
                ErrorMessage = "Please select a client to update";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var request = new UpdateBankClientRequest
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    DateOfBirth = DateOfBirth,
                    NationalId = NationalId,
                    Street = Street,
                    City = City,
                    PostalCode = PostalCode,
                    Country = Country
                };

                _logger.LogInformation("Updating bank client {ClientId}", SelectedClient.Id);
                var updated = await _clientApiService.UpdateClientAsync(SelectedClient.Id, request);
                if (updated != null)
                {
                    var index = Clients.IndexOf(SelectedClient);
                    if (index >= 0)
                    {
                        Clients[index] = updated;
                        SelectedClient = updated;
                    }
                    ErrorMessage = "Client updated successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating client: {ex.Message}";
                _logger.LogError("Failed to update bank client: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SuspendClient()
        {
            if (SelectedClient == null)
            {
                ErrorMessage = "Please select a client to suspend";
                return;
            }

            if (SelectedClient.Status == ClientStatus.Suspended)
            {
                ErrorMessage = "Client is already suspended";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Suspending bank client {ClientId}", SelectedClient.Id);

                var updated = await _clientApiService.SuspendClientAsync(SelectedClient.Id);
                if (updated != null)
                {
                    var index = Clients.IndexOf(SelectedClient);
                    if (index >= 0)
                    {
                        Clients[index] = updated;
                        SelectedClient = updated;
                    }
                    ErrorMessage = "Client suspended successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error suspending client: {ex.Message}";
                _logger.LogError("Failed to suspend bank client: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task ActivateClient()
        {
            if (SelectedClient == null)
            {
                ErrorMessage = "Please select a client to activate";
                return;
            }

            if (SelectedClient.Status == ClientStatus.Active)
            {
                ErrorMessage = "Client is already active";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Activating bank client {ClientId}", SelectedClient.Id);

                var updated = await _clientApiService.ActivateClientAsync(SelectedClient.Id);
                if (updated != null)
                {
                    var index = Clients.IndexOf(SelectedClient);
                    if (index >= 0)
                    {
                        Clients[index] = updated;
                        SelectedClient = updated;
                    }
                    ErrorMessage = "Client activated successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error activating client: {ex.Message}";
                _logger.LogError("Failed to activate bank client: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task DeleteClient()
        {
            if (SelectedClient == null)
            {
                ErrorMessage = "Please select a client to delete";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Deleting bank client {ClientId}", SelectedClient.Id);

                var deleted = await _clientApiService.DeleteClientAsync(SelectedClient.Id);
                if (deleted)
                {
                    Clients.Remove(SelectedClient);
                    SelectedClient = null;
                    ClearForm();
                    ErrorMessage = "Client deleted successfully";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting client: {ex.Message}";
                _logger.LogError("Failed to delete bank client: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSelectedClientChanged(BankClientDto? value)
        {
            if (value is null)
                return;

            FirstName = value.FirstName;
            LastName = value.LastName;
            Email = value.Email;
            PhoneNumber = value.PhoneNumber;
            DateOfBirth = value.DateOfBirth;
            NationalId = value.NationalId;
            Street = value.Street;
            City = value.City;
            PostalCode = value.PostalCode;
            Country = value.Country;
        }

        private void ClearForm()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            DateOfBirth = new DateTime(1990, 1, 1);
            NationalId = string.Empty;
            Street = string.Empty;
            City = string.Empty;
            PostalCode = string.Empty;
            Country = string.Empty;
        }
    }
}
