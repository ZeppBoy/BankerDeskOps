using BankerDeskOps.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Wpf.Services
{
    public class ProductApiService
    {
        private readonly ApiClient _apiClient;
        private readonly ILogger<ProductApiService> _logger;
        private const string Endpoint = "api/products";

        public ProductApiService(ApiClient apiClient, ILogger<ProductApiService> logger)
        {
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all products");
                var products = await _apiClient.GetAsync<IEnumerable<ProductDto>>(Endpoint);
                return products ?? Enumerable.Empty<ProductDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching products: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ProductDto?> GetProductByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching product {ProductId}", id);
                return await _apiClient.GetAsync<ProductDto>($"{Endpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching product {ProductId}: {Message}", id, ex.Message);
                throw;
            }
        }

        public async Task<ProductDto?> CreateProductAsync(CreateProductRequest request)
        {
            try
            {
                _logger.LogInformation("Creating product {Name}", request.Name);
                return await _apiClient.PostAsync<CreateProductRequest, ProductDto>(Endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating product: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<ProductDto?> UpdateProductAsync(UpdateProductRequest request)
        {
            try
            {
                _logger.LogInformation("Updating product {ProductId}", request.Id);
                return await _apiClient.PutAsync<UpdateProductRequest, ProductDto>(Endpoint, request);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating product: {Message}", ex.Message);
                throw;
            }
        }

        public async Task DeleteProductAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting product {ProductId}", id);
                await _apiClient.DeleteAsync($"{Endpoint}/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting product {ProductId}: {Message}", id, ex.Message);
                throw;
            }
        }
    }
}
