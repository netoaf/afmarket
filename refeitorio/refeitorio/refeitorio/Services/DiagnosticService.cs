using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace refeitorio.Services
{
    public class DiagnosticService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public DiagnosticService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7091/api";
        }

        public async Task<ApiHealthStatus> CheckApiConnectivity()
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                };

                using (var client = new HttpClient(handler))
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = await client.GetAsync($"{_baseUrl}/health");

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        return new ApiHealthStatus
                        {
                            IsOnline = true,
                            Message = "API está online",
                            Details = content
                        };
                    }
                    else
                    {
                        return new ApiHealthStatus
                        {
                            IsOnline = false,
                            Message = $"API retornou status code: {(int)response.StatusCode} {response.StatusCode}",
                            Details = await response.Content.ReadAsStringAsync()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ApiHealthStatus
                {
                    IsOnline = false,
                    Message = $"Não foi possível conectar à API: {ex.Message}",
                    Details = ex.StackTrace
                };
            }
        }
    }

    public class ApiHealthStatus
    {
        public bool IsOnline { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
