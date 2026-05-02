using BankerDeskOps.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    public class CommissionApiService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<CommissionApiService> _logger;
        private const string Endpoint = "api/commissions";

        public CommissionApiService(ApiClient apiClient, ILogger<CommissionApiService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<CommissionDto>> GetAllCommissionsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all commissions");
                var commissions = await _apiClient.GetAsync<IEnumerable<CommissionDto>>(Endpoint);
                return commissions ?? Enumerable.Empty<CommissionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching commissions: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<CommissionDto>> GetCommissionsByProductIdAsync(Guid productId)
        {
            try
            {
                _logger.LogInformation("Fetching commissions for product {ProductId}", productId);
                var commissions = await _apiClient.GetAsync<IEnumerable<CommissionDto>>($"{Endpoint}/product/{productId}");
                return commissions ?? Enumerable.Empty<CommissionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching commissions for product {ProductId}: {Message}", productId, ex.Message);
                throw;
            }
        }

        public async Task<CommissionDto?> CreateCommissionAsync(CreateCommissionRequest request)
        {
            try
            {
                _logger.LogInformation("Creating commission {Name}", request.Name);
                return await _apiClient.PostAsync<CreateCommissionRequest, CommissionDto>(Endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating commission: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<CommissionDto?> UpdateCommissionAsync(UpdateCommissionRequest request)
        {
            try
            {
                _logger.LogInformation("Updating commission {CommissionId}", request.Id);
                return await _apiClient.PutAsync<UpdateCommissionRequest, CommissionDto>(Endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating commission: {Message}", ex.Message);
                throw;
            }
        }

        public async Task DeleteCommissionAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting commission {CommissionId}", id);
                await _apiClient.DeleteAsync($"{Endpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting commission {CommissionId}: {Message}", id, ex.Message);
                throw;
            }
        }
    }
}
