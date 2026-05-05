using BankerDeskOps.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    public class LoanApplicationApiService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<LoanApplicationApiService> _logger;
        private const string Endpoint = "api/loan-applications";

        public LoanApplicationApiService(ApiClient apiClient, ILogger<LoanApplicationApiService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LoanApplicationDto>> GetAllApplicationsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all loan applications");
                var applications = await _apiClient.GetAsync<IEnumerable<LoanApplicationDto>>(Endpoint);
                return applications ?? Enumerable.Empty<LoanApplicationDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching loan applications: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<LoanApplicationDto?> GetApplicationByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching loan application {ApplicationId}", id);
                return await _apiClient.GetAsync<LoanApplicationDto>($"{Endpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching loan application {ApplicationId}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task UpdateStatusAsync(Guid id, string status, string? comment = null)
        {
            try
            {
                _logger.LogInformation("Updating loan application {ApplicationId} status to {Status}", id, status);
                await _apiClient.PutAsync<object, object>($"{Endpoint}/{id}/status", new { Status = status, Comment = comment });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating loan application status: {Message}", ex.Message);
                throw;
            }
        }

        public async Task DeleteApplicationAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting loan application {ApplicationId}", id);
                await _apiClient.DeleteAsync($"{Endpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting loan application {ApplicationId}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<LoanApplicationDto?> CreateLoanApplicationAsync(CreateLoanApplicationRequest request)
        {
            try
            {
                _logger.LogInformation("Creating loan application for product {ProductId}", request.ProductId);
                return await _apiClient.PostAsync<CreateLoanApplicationRequest, LoanApplicationDto>(Endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating loan application: {Message}", ex.Message);
                throw;
            }
        }
    }
}
