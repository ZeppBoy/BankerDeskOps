using BankerDeskOps.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    public class RateApiService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<RateApiService> _logger;
        private const string Endpoint = "api/rates";

        public RateApiService(ApiClient apiClient, ILogger<RateApiService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<RateDto>> GetAllRatesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all rates");
                var rates = await _apiClient.GetAsync<IEnumerable<RateDto>>(Endpoint);
                return rates ?? Enumerable.Empty<RateDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching rates: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<RateDto>> GetRatesByProductIdAsync(Guid productId)
        {
            try
            {
                _logger.LogInformation("Fetching rates for product {ProductId}", productId);
                var rates = await _apiClient.GetAsync<IEnumerable<RateDto>>($"{Endpoint}/product/{productId}");
                return rates ?? Enumerable.Empty<RateDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching rates for product {ProductId}: {Message}", productId, ex.Message);
                throw;
            }
        }

        public async Task<RateDto?> CreateRateAsync(CreateRateRequest request)
        {
            try
            {
                _logger.LogInformation("Creating rate");
                return await _apiClient.PostAsync<CreateRateRequest, RateDto>(Endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating rate: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<RateDto?> UpdateRateAsync(UpdateRateRequest request)
        {
            try
            {
                _logger.LogInformation("Updating rate {RateId}", request.Id);
                return await _apiClient.PutAsync<UpdateRateRequest, RateDto>(Endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating rate: {Message}", ex.Message);
                throw;
            }
        }

        public async Task DeleteRateAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting rate {RateId}", id);
                await _apiClient.DeleteAsync($"{Endpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting rate {RateId}: {Message}", id, ex.Message);
                throw;
            }
        }
    }
}
