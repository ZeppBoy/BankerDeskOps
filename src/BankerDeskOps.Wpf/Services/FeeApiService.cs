using BankerDeskOps.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    public class FeeApiService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<FeeApiService> _logger;
        private const string Endpoint = "api/fees";

        public FeeApiService(ApiClient apiClient, ILogger<FeeApiService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<FeeDto>> GetAllFeesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all fees");
                var fees = await _apiClient.GetAsync<IEnumerable<FeeDto>>(Endpoint);
                return fees ?? Enumerable.Empty<FeeDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching fees: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<FeeDto>> GetFeesByProductIdAsync(Guid productId)
        {
            try
            {
                _logger.LogInformation("Fetching fees for product {ProductId}", productId);
                var fees = await _apiClient.GetAsync<IEnumerable<FeeDto>>($"{Endpoint}/product/{productId}");
                return fees ?? Enumerable.Empty<FeeDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching fees for product {ProductId}: {Message}", productId, ex.Message);
                throw;
            }
        }

        public async Task<FeeDto?> CreateFeeAsync(CreateFeeRequest request)
        {
            try
            {
                _logger.LogInformation("Creating fee {Name}", request.Name);
                return await _apiClient.PostAsync<CreateFeeRequest, FeeDto>(Endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating fee: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<FeeDto?> UpdateFeeAsync(UpdateFeeRequest request)
        {
            try
            {
                _logger.LogInformation("Updating fee {FeeId}", request.Id);
                return await _apiClient.PutAsync<UpdateFeeRequest, FeeDto>(Endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating fee: {Message}", ex.Message);
                throw;
            }
        }

        public async Task DeleteFeeAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting fee {FeeId}", id);
                await _apiClient.DeleteAsync($"{Endpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting fee {FeeId}: {Message}", id, ex.Message);
                throw;
            }
        }
    }
}
