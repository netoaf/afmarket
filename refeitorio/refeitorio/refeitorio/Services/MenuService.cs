using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using refeitorio.ViewModels;

namespace refeitorio.Services
{
    public class MenuService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger<MenuService> _logger;

        public MenuService(IConfiguration configuration, ILogger<MenuService> logger)
        {
            _logger = logger;
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler);
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7091/api";
            _logger.LogInformation($"MenuService initialized with BaseUrl: {_baseUrl}");
        }

        public async Task<List<WeeklyMenuItemViewModel>> GetWeeklyMenu(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                string endpoint = "WeeklyMenu";
                bool hasParam = false;
                
                if (startDate.HasValue || endDate.HasValue)
                {
                    endpoint += "?";
                    
                    if (startDate.HasValue)
                    {
                        endpoint += $"startDate={startDate.Value:yyyy-MM-dd}";
                        hasParam = true;
                    }
                    
                    if (endDate.HasValue)
                    {
                        endpoint += hasParam ? "&" : "";
                        endpoint += $"endDate={endDate.Value:yyyy-MM-dd}";
                    }
                }
                
                var url = $"{_baseUrl}/{endpoint}";
                _logger.LogInformation($"Fetching weekly menu from: {url}");
                
                var response = await _httpClient.GetAsync(url);
                
                _logger.LogInformation($"Response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var menu = await response.Content.ReadFromJsonAsync<List<WeeklyMenuItemViewModel>>();
                    _logger.LogInformation($"Retrieved {menu?.Count ?? 0} menu items");
                    return menu ?? new List<WeeklyMenuItemViewModel>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to get weekly menu. Status: {response.StatusCode}, Error: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception getting weekly menu: {ex.Message}");
            }
            
            return new List<WeeklyMenuItemViewModel>();
        }

        public async Task<WeeklyMenuItemViewModel> GetMenuItem(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/WeeklyMenu/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WeeklyMenuItemViewModel>();
            }
            
            return null;
        }

        public async Task<TicketViewModel> CreateTicket(int userId, int menuId)
        {
            var ticketData = new
            {
                UserId = userId,
                MenuId = menuId
            };

            var content = new StringContent(JsonSerializer.Serialize(ticketData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/Tickets", content);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TicketViewModel>();
            }
            
            return null;
        }

        public async Task<List<TicketViewModel>> GetUserTickets(int userId, DateTime? date = null)
        {
            string endpoint = $"Tickets?userId={userId}";
            
            if (date.HasValue)
            {
                endpoint += $"&date={date.Value:yyyy-MM-dd}";
            }
            
            var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<TicketViewModel>>();
            }
            
            return new List<TicketViewModel>();
        }

        public async Task<bool> CancelTicket(int ticketId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/Tickets/{ticketId}");
            return response.IsSuccessStatusCode;
        }
    }
}
