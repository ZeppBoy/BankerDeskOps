using BankerDeskOps.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    /// <summary>
    /// Service for interacting with retail account API endpoints.
    /// </summary>
    public class RetailAccountApiService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<RetailAccountApiService> _logger;
        private const string AccountsEndpoint = "api/retailaccounts";

        public RetailAccountApiService(ApiClient apiClient, ILogger<RetailAccountApiService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retrieves all retail accounts.
        /// </summary>
        public async Task<IEnumerable<RetailAccountDto>> GetAllAccountsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all retail accounts");
                var accounts = await _apiClient.GetAsync<IEnumerable<RetailAccountDto>>(AccountsEndpoint);
                return accounts ?? Enumerable.Empty<RetailAccountDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching accounts: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Retrieves an account by ID.
        /// </summary>
        public async Task<RetailAccountDto?> GetAccountByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching account {AccountId}", id);
                return await _apiClient.GetAsync<RetailAccountDto>($"{AccountsEndpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching account {AccountId}: {Message}", id, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Opens a new retail account.
        /// </summary>
        public async Task<RetailAccountDto?> OpenAccountAsync(CreateRetailAccountRequest request)
        {
            try
            {
                _logger.LogInformation("Opening account for customer {CustomerName}", request.CustomerName);
                return await _apiClient.PostAsync<CreateRetailAccountRequest, RetailAccountDto>(AccountsEndpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error opening account: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Deposits funds into an account.
        /// </summary>
        public async Task<RetailAccountDto?> DepositAsync(Guid id, DepositRequest request)
        {
            try
            {
                _logger.LogInformation("Depositing {Amount} to account {AccountId}", request.Amount, id);
                return await _apiClient.PostAsync<DepositRequest, RetailAccountDto>($"{AccountsEndpoint}/{id}/deposit", request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error depositing to account {AccountId}: {Message}", id, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Withdraws funds from an account.
        /// </summary>
        public async Task<RetailAccountDto?> WithdrawAsync(Guid id, WithdrawRequest request)
        {
            try
            {
                _logger.LogInformation("Withdrawing {Amount} from account {AccountId}", request.Amount, id);
                return await _apiClient.PostAsync<WithdrawRequest, RetailAccountDto>($"{AccountsEndpoint}/{id}/withdraw", request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error withdrawing from account {AccountId}: {Message}", id, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Closes an account.
        /// </summary>
        public async Task CloseAccountAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Closing account {AccountId}", id);
                await _apiClient.DeleteAsync($"{AccountsEndpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error closing account {AccountId}: {Message}", id, ex.Message);
                throw;
            }
        }
    }
}
