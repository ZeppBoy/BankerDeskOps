using BankerDeskOps.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    public class CurrencyApiService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<CurrencyApiService> _logger;
        private const string Endpoint = "api/currencies";

        public CurrencyApiService(ApiClient apiClient, ILogger<CurrencyApiService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<CurrencyDto>> GetAllCurrenciesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all currencies");
                var currencies = await _apiClient.GetAsync<IEnumerable<CurrencyDto>>(Endpoint);
                return currencies ?? Enumerable.Empty<CurrencyDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching currencies: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<CurrencyDto?> GetCurrencyByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching currency {CurrencyId}", id);
                return await _apiClient.GetAsync<CurrencyDto>($"{Endpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching currency {CurrencyId}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<CurrencyDto?> CreateCurrencyAsync(CreateCurrencyRequest request)
        {
            try
            {
                _logger.LogInformation("Creating currency {Code}", request.Code);
                return await _apiClient.PostAsync<CreateCurrencyRequest, CurrencyDto>(Endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating currency: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<CurrencyDto?> UpdateCurrencyAsync(UpdateCurrencyRequest request)
        {
            try
            {
                _logger.LogInformation("Updating currency {CurrencyId}", request.Id);
                return await _apiClient.PutAsync<UpdateCurrencyRequest, CurrencyDto>(Endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating currency: {Message}", ex.Message);
                throw;
            }
        }

        public async Task DeleteCurrencyAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting currency {CurrencyId}", id);
                await _apiClient.DeleteAsync($"{Endpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting currency {CurrencyId}: {Message}", id, ex.Message);
                throw;
            }
        }
    }
}
