using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace BankerDeskOps.Avalonia.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string BaseAddress
        {
            get => _httpClient.BaseAddress?.ToString() ?? string.Empty;
            set => _httpClient.BaseAddress = new Uri(value);
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                _logger.LogInformation("GET request to {Endpoint}", endpoint);
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<T>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("HTTP error on GET {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            try
            {
                _logger.LogInformation("POST request to {Endpoint}", endpoint);
                var response = await _httpClient.PostAsJsonAsync(endpoint, request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("HTTP error on POST {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
        {
            try
            {
                _logger.LogInformation("PUT request to {Endpoint}", endpoint);
                var response = await _httpClient.PutAsJsonAsync(endpoint, request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("HTTP error on PUT {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }

        public async Task DeleteAsync(string endpoint)
        {
            try
            {
                _logger.LogInformation("DELETE request to {Endpoint}", endpoint);
                var response = await _httpClient.DeleteAsync(endpoint);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("HTTP error on DELETE {Endpoint}: {Message}", endpoint, ex.Message);
                throw;
            }
        }
    }
}
