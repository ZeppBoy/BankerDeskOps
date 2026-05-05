using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BankerDeskOps.Wpf.ViewModels
{
    public partial class RepaymentScheduleViewModel : ObservableObject, INotifyDataErrorInfo
    {
        private readonly LoanApplicationApiService _applicationApiService;
        private readonly RepaymentScheduleApiService _scheduleApiService;
        private readonly ILogger<RepaymentScheduleViewModel> _logger;
        private Dictionary<string, List<string>> _errors = new();

        [ObservableProperty]
        private ObservableCollection<LoanApplicationDto> applications = new();

        [ObservableProperty]
        private LoanApplicationDto? selectedApplication;

        [ObservableProperty]
        private ObservableCollection<RepaymentScheduleDto> schedules = new();

        [ObservableProperty]
        private RepaymentScheduleDto? selectedSchedule;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        // New entry form fields
        [ObservableProperty]
        private int paymentNumber = 1;

        [ObservableProperty]
        private DateTime dueDate = DateTime.Today;

        [ObservableProperty]
        private decimal principalAmount = 0m;

        [ObservableProperty]
        private decimal interestAmount = 0m;

        [ObservableProperty]
        private decimal totalPayment = 0m;

        public bool HasErrors => _errors.Values.Any(e => e.Count > 0);
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public RepaymentScheduleViewModel(
            LoanApplicationApiService applicationApiService,
            RepaymentScheduleApiService scheduleApiService,
            ILogger<RepaymentScheduleViewModel> logger)
        {
            _applicationApiService = applicationApiService ?? throw new ArgumentNullException(nameof(applicationApiService));
            _scheduleApiService = scheduleApiService ?? throw new ArgumentNullException(nameof(scheduleApiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [RelayCommand]
        public async Task LoadAll()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var applications = await _applicationApiService.GetAllApplicationsAsync();
                Applications.Clear();
                foreach (var app in applications)
                {
                    Applications.Add(app);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading data: {ex.Message}";
                _logger.LogError("Failed to load repayment schedule data: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task LoadSchedulesForApplication()
        {
            if (SelectedApplication == null)
            {
                ErrorMessage = "Please select a loan application first.";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Loading schedules for application {AppId}", SelectedApplication.Id);

                var schedules = await _scheduleApiService.GetSchedulesByLoanApplicationAsync(SelectedApplication.Id);
                Schedules.Clear();
                foreach (var schedule in schedules)
                {
                    Schedules.Add(schedule);
                }

                // Auto-set next payment number
                PaymentNumber = Schedules.Count > 0 ? Schedules.Max(s => s.PaymentNumber) + 1 : 1;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading schedules: {ex.Message}";
                _logger.LogError("Failed to load schedules: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task AddEntry()
        {
            if (SelectedApplication == null)
            {
                ErrorMessage = "Please select a loan application first.";
                return;
            }

            var validationErrors = ValidateEntry();
            if (validationErrors.Count > 0)
            {
                ErrorMessage = string.Join("; ", validationErrors.SelectMany(e => e.Value));
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var request = new CreateRepaymentScheduleRequest
                {
                    LoanApplicationId = SelectedApplication.Id,
                    PaymentNumber = PaymentNumber,
                    DueDate = DueDate,
                    PrincipalAmount = PrincipalAmount,
                    InterestAmount = InterestAmount,
                    TotalPayment = TotalPayment
                };

                _logger.LogInformation("Adding schedule entry #{Num} for application {AppId}", PaymentNumber, SelectedApplication.Id);
                var created = await _scheduleApiService.CreateScheduleAsync(request);

                if (created != null)
                {
                    Schedules.Add(created);
                    ErrorMessage = $"Entry #{PaymentNumber} added successfully.";
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding entry: {ex.Message}";
                _logger.LogError("Failed to add schedule entry: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task DeleteEntry()
        {
            if (SelectedSchedule == null)
            {
                ErrorMessage = "Please select a schedule entry to delete.";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                _logger.LogInformation("Deleting schedule entry {Id}", SelectedSchedule.ScheduleId);
                await _scheduleApiService.DeleteScheduleAsync(SelectedSchedule.ScheduleId);
                Schedules.Remove(SelectedSchedule);
                SelectedSchedule = null;
                ErrorMessage = "Entry deleted successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting entry: {ex.Message}";
                _logger.LogError("Failed to delete schedule entry: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void ClearForm()
        {
            PaymentNumber = Schedules.Count > 0 ? Schedules.Max(s => s.PaymentNumber) + 1 : 1;
            DueDate = DateTime.Today;
            PrincipalAmount = 0m;
            InterestAmount = 0m;
            TotalPayment = 0m;
        }

        private Dictionary<string, List<string>> ValidateEntry()
        {
            var errors = new Dictionary<string, List<string>>();

            if (PaymentNumber <= 0)
                errors["PaymentNumber"] = new List<string> { "Payment number must be greater than 0." };

            if (DueDate < DateTime.Today.AddDays(-1))
                errors["DueDate"] = new List<string> { "Due date cannot be in the past." };

            if (PrincipalAmount < 0)
                errors["PrincipalAmount"] = new List<string> { "Principal amount cannot be negative." };

            if (InterestAmount < 0)
                errors["InterestAmount"] = new List<string> { "Interest amount cannot be negative." };

            if (TotalPayment <= 0)
                errors["TotalPayment"] = new List<string> { "Total payment must be greater than 0." };

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
    }
}
