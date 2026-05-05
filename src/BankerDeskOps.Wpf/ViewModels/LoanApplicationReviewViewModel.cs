using BankerDeskOps.Application.DTOs;
using BankerDeskOps.Wpf.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace BankerDeskOps.Wpf.ViewModels
{
    public partial class LoanApplicationReviewViewModel : ObservableObject, INotifyDataErrorInfo
    {
        private readonly LoanApplicationApiService _applicationApiService;
        private readonly RepaymentScheduleApiService _scheduleApiService;
        private readonly ILogger<LoanApplicationReviewViewModel> _logger;
        private Dictionary<string, List<string>> _errors = new();

        [ObservableProperty]
        private ObservableCollection<LoanApplicationDto> applications = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string? errorMessage;

        [ObservableProperty]
        private LoanApplicationDto? selectedApplication;

        [ObservableProperty]
        private string statusComment = string.Empty;

        [ObservableProperty]
        private string filterStatus = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> uploadedDocumentNames = new();

        // Repayment Schedule properties
        [ObservableProperty]
        private ObservableCollection<RepaymentScheduleDto> repaymentSchedules = new();

        [ObservableProperty]
        private RepaymentScheduleDto? selectedSchedule;

        [ObservableProperty]
        private int newPaymentNumber = 1;

        [ObservableProperty]
        private DateTime newDueDate = DateTime.Today;

        [ObservableProperty]
        private decimal newPrincipalAmount = 0m;

        [ObservableProperty]
        private decimal newInterestAmount = 0m;

        [ObservableProperty]
        private decimal newTotalPayment = 0m;

        public bool HasErrors => _errors.Values.Any(e => e.Count > 0);
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public LoanApplicationReviewViewModel(LoanApplicationApiService applicationApiService, RepaymentScheduleApiService scheduleApiService, ILogger<LoanApplicationReviewViewModel> logger)
        {
            _applicationApiService = applicationApiService ?? throw new ArgumentNullException(nameof(applicationApiService));
            _scheduleApiService = scheduleApiService ?? throw new ArgumentNullException(nameof(scheduleApiService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [RelayCommand]
        public async Task LoadApplications()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Loading loan applications");

                var applications = await _applicationApiService.GetAllApplicationsAsync();
                Applications.Clear();
                foreach (var app in applications)
                {
                    if (string.IsNullOrWhiteSpace(FilterStatus) || app.Status.Equals(FilterStatus, StringComparison.OrdinalIgnoreCase))
                        Applications.Add(app);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading loan applications: {ex.Message}";
                _logger.LogError("Failed to load loan applications: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task ApproveApplication()
        {
            if (SelectedApplication == null)
            {
                ErrorMessage = "Please select an application to approve";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Approving loan application {ApplicationId}", SelectedApplication.Id);

                await _applicationApiService.UpdateStatusAsync(SelectedApplication.Id, "Approved", StatusComment);
                SelectedApplication.Status = "Approved";
                SelectedApplication.UpdatedAt = DateTime.UtcNow;
                ErrorMessage = "Application approved successfully";
                StatusComment = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error approving application: {ex.Message}";
                _logger.LogError("Failed to approve application: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task RejectApplication()
        {
            if (SelectedApplication == null)
            {
                ErrorMessage = "Please select an application to reject";
                return;
            }

            var validationErrors = ValidateComment();
            if (validationErrors.Count > 0)
            {
                ErrorMessage = string.Join("; ", validationErrors.SelectMany(e => e.Value));
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Rejecting loan application {ApplicationId}", SelectedApplication.Id);

                await _applicationApiService.UpdateStatusAsync(SelectedApplication.Id, "Rejected", StatusComment);
                SelectedApplication.Status = "Rejected";
                SelectedApplication.UpdatedAt = DateTime.UtcNow;
                ErrorMessage = "Application rejected successfully";
                StatusComment = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error rejecting application: {ex.Message}";
                _logger.LogError("Failed to reject application: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task DeleteApplication()
        {
            if (SelectedApplication == null)
            {
                ErrorMessage = "Please select an application to delete";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;
                _logger.LogInformation("Deleting loan application {ApplicationId}", SelectedApplication.Id);

                await _applicationApiService.DeleteApplicationAsync(SelectedApplication.Id);
                Applications.Remove(SelectedApplication);
                SelectedApplication = null;
                ErrorMessage = "Application deleted successfully";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting application: {ex.Message}";
                _logger.LogError("Failed to delete application: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void UploadDocument()
        {
            if (SelectedApplication == null)
            {
                ErrorMessage = "Please select an application first";
                return;
            }

            try
            {
                var dialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Filter = "Supported Documents|*.pdf;*.doc;*.docx;*.jpg;*.png;*.jpeg|PDF Files|*.pdf|Word Documents|*.doc;*.docx|Image Files|*.jpg;*.jpeg;*.png|All Files|*.*",
                    Title = "Select documents to upload"
                };

                if (dialog.ShowDialog() == true)
                {
                    foreach (var fileName in dialog.FileNames)
                    {
                        var fileInfo = new FileInfo(fileName);

                        if (fileInfo.Length > 10 * 1024 * 1024)
                        {
                            ErrorMessage = $"File {fileInfo.Name} exceeds the 10MB size limit.";
                            _logger.LogWarning("File {FileName} exceeds size limit", fileInfo.Name);
                            continue;
                        }

                        UploadedDocumentNames.Add(fileInfo.Name);
                        _logger.LogInformation("Document selected for upload: {FileName}", fileInfo.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error selecting documents: {ex.Message}";
                _logger.LogError("Failed to select documents: {Message}", ex.Message);
            }
        }

        [RelayCommand]
        public void RemoveDocument()
        {
            if (UploadedDocumentNames.Count > 0)
            {
                UploadedDocumentNames.RemoveAt(UploadedDocumentNames.Count - 1);
            }
        }

        private Dictionary<string, List<string>> ValidateComment()
        {
            var errors = new Dictionary<string, List<string>>();

            if (string.IsNullOrWhiteSpace(StatusComment))
                errors["StatusComment"] = new List<string> { "A comment is required when rejecting an application." };

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

        // --- Repayment Schedule Commands ---

        [RelayCommand]
        public async Task LoadSchedules()
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
                _logger.LogInformation("Loading repayment schedules for application {ApplicationId}", SelectedApplication.Id);

                var schedules = await _scheduleApiService.GetSchedulesByLoanApplicationAsync(SelectedApplication.Id);
                RepaymentSchedules.Clear();
                foreach (var schedule in schedules)
                {
                    RepaymentSchedules.Add(schedule);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading repayment schedules: {ex.Message}";
                _logger.LogError("Failed to load repayment schedules: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task AddScheduleEntry()
        {
            if (SelectedApplication == null)
            {
                ErrorMessage = "Please select a loan application first.";
                return;
            }

            var validationErrors = ValidateScheduleEntry();
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
                    PaymentNumber = NewPaymentNumber,
                    DueDate = NewDueDate,
                    PrincipalAmount = NewPrincipalAmount,
                    InterestAmount = NewInterestAmount,
                    TotalPayment = NewTotalPayment
                };

                _logger.LogInformation("Adding repayment schedule entry #{PaymentNumber} for application {ApplicationId}",
                    NewPaymentNumber, SelectedApplication.Id);

                var created = await _scheduleApiService.CreateScheduleAsync(request);
                if (created != null)
                {
                    RepaymentSchedules.Add(created);
                    ErrorMessage = $"Schedule entry #{NewPaymentNumber} added successfully.";
                    ClearScheduleForm();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error adding schedule entry: {ex.Message}";
                _logger.LogError("Failed to add repayment schedule entry: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task DeleteScheduleEntry()
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

                _logger.LogInformation("Deleting repayment schedule entry {ScheduleId}", SelectedSchedule.ScheduleId);
                await _scheduleApiService.DeleteScheduleAsync(SelectedSchedule.ScheduleId);
                RepaymentSchedules.Remove(SelectedSchedule);
                SelectedSchedule = null;
                ErrorMessage = "Schedule entry deleted successfully.";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error deleting schedule entry: {ex.Message}";
                _logger.LogError("Failed to delete repayment schedule entry: {Message}", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void ClearScheduleForm()
        {
            NewPaymentNumber = RepaymentSchedules.Count + 1;
            NewDueDate = DateTime.Today;
            NewPrincipalAmount = 0m;
            NewInterestAmount = 0m;
            NewTotalPayment = 0m;
        }

        private Dictionary<string, List<string>> ValidateScheduleEntry()
        {
            var errors = new Dictionary<string, List<string>>();

            if (NewPaymentNumber <= 0)
                errors["PaymentNumber"] = new List<string> { "Payment number must be greater than 0." };

            if (NewDueDate <= DateTime.Today.AddDays(-1))
                errors["DueDate"] = new List<string> { "Due date cannot be in the past." };

            if (NewPrincipalAmount < 0)
                errors["PrincipalAmount"] = new List<string> { "Principal amount cannot be negative." };

            if (NewInterestAmount < 0)
                errors["InterestAmount"] = new List<string> { "Interest amount cannot be negative." };

            if (NewTotalPayment <= 0)
                errors["TotalPayment"] = new List<string> { "Total payment must be greater than 0." };

            _errors = errors;
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(string.Empty));
            return errors;
        }
    }
}
