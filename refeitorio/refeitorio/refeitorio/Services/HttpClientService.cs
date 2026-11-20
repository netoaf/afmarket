using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace refeitorio.Services
{
    public class HttpClientService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HttpClientService> _logger;
        private readonly string _baseUrl;

        public HttpClientService(IConfiguration configuration, ILogger<HttpClientService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7091/api";
        }

        public HttpClient CreateClient()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            return client;
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            try
            {
                using var client = CreateClient();
                _logger.LogInformation($"GET Request: {_baseUrl}/{endpoint}");
                
                var response = await client.GetAsync($"{_baseUrl}/{endpoint}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response: {content}");
                
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GET {endpoint}: {ex.Message}");
                throw;
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                using var client = CreateClient();
                var json = JsonSerializer.Serialize(data);
                _logger.LogInformation($"POST Request to {_baseUrl}/{endpoint}: {json}");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_baseUrl}/{endpoint}", content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response: {responseContent}");
                
                return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in POST {endpoint}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                using var client = CreateClient();
                _logger.LogInformation($"DELETE Request: {_baseUrl}/{endpoint}");
                
                var response = await client.DeleteAsync($"{_baseUrl}/{endpoint}");
                response.EnsureSuccessStatusCode();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DELETE {endpoint}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PutAsync(string endpoint, object data)
        {
            try
            {
                using var client = CreateClient();
                var json = JsonSerializer.Serialize(data);
                _logger.LogInformation($"PUT Request to {_baseUrl}/{endpoint}: {json}");
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"{_baseUrl}/{endpoint}", content);
                response.EnsureSuccessStatusCode();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PUT {endpoint}: {ex.Message}");
                return false;
            }
        }
    }
}
