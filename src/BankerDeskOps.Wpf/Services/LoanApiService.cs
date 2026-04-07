using BankerDeskOps.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    /// <summary>
    /// Service for interacting with loan API endpoints.
    /// </summary>
    public class LoanApiService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<LoanApiService> _logger;
        private const string LoansEndpoint = "api/loans";

        public LoanApiService(ApiClient apiClient, ILogger<LoanApiService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all loans.
        /// </summary>
        public async Task<IEnumerable<LoanDto>> GetAllLoansAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all loans");
                var loans = await _apiClient.GetAsync<IEnumerable<LoanDto>>(LoansEndpoint);
                return loans ?? Enumerable.Empty<LoanDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching loans: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a loan by ID.
        /// </summary>
        public async Task<LoanDto?> GetLoanByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching loan {LoanId}", id);
                return await _apiClient.GetAsync<LoanDto>($"{LoansEndpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching loan {LoanId}: {Message}", id, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Creates a new loan.
        /// </summary>
        public async Task<LoanDto?> CreateLoanAsync(CreateLoanRequest request)
        {
            try
            {
                _logger.LogInformation("Creating loan for customer {CustomerName}", request.CustomerName);
                return await _apiClient.PostAsync<CreateLoanRequest, LoanDto>(LoansEndpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating loan: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Approves a loan.
        /// </summary>
        public async Task<LoanDto?> ApproveLoanAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Approving loan {LoanId}", id);
                return await _apiClient.PutAsync<object, LoanDto>($"{LoansEndpoint}/{id}/approve", new { });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error approving loan {LoanId}: {Message}", id, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Rejects a loan.
        /// </summary>
        public async Task<LoanDto?> RejectLoanAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Rejecting loan {LoanId}", id);
                return await _apiClient.PutAsync<object, LoanDto>($"{LoansEndpoint}/{id}/reject", new { });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error rejecting loan {LoanId}: {Message}", id, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Deletes a loan.
        /// </summary>
        public async Task DeleteLoanAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting loan {LoanId}", id);
                await _apiClient.DeleteAsync($"{LoansEndpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting loan {LoanId}: {Message}", id, ex.Message);
                throw;
            }
        }
    }
}
