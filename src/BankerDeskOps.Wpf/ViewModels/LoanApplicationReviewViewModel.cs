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

        public bool HasErrors => _errors.Values.Any(e => e.Count > 0);
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public LoanApplicationReviewViewModel(LoanApplicationApiService applicationApiService, ILogger<LoanApplicationReviewViewModel> logger)
        {
            _applicationApiService = applicationApiService ?? throw new ArgumentNullException(nameof(applicationApiService));
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
    }
}
