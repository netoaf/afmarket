using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using refeitorio.ViewModels;

namespace refeitorio.Services
{
    public class MenuTicketService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger<MenuTicketService> _logger;

        public MenuTicketService(IConfiguration configuration, ILogger<MenuTicketService> logger)
        {
            _logger = logger;
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler);
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7091/api";
            _logger.LogInformation($"MenuTicketService initialized with BaseUrl: {_baseUrl}");
        }

        public async Task<List<WeeklyMenuItemViewModel>> GetWeeklyMenuAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isVegetarian = null)
        {
            try
            {
                string endpoint = "WeeklyMenu";
                bool hasParam = false;
                
                if (startDate.HasValue)
                {
                    endpoint += $"?startDate={startDate.Value:yyyy-MM-dd}";
                    hasParam = true;
                }
                
                if (endDate.HasValue)
                {
                    endpoint += hasParam ? $"&endDate={endDate.Value:yyyy-MM-dd}" : $"?endDate={endDate.Value:yyyy-MM-dd}";
                    hasParam = true;
                }
                
                if (isVegetarian.HasValue)
                {
                    endpoint += hasParam ? $"&isVegetarian={isVegetarian.Value}" : $"?isVegetarian={isVegetarian.Value}";
                }
                
                var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");
                
                if (response.IsSuccessStatusCode)
                {
                    var menu = await response.Content.ReadFromJsonAsync<List<WeeklyMenuItemViewModel>>();
                    return menu ?? new List<WeeklyMenuItemViewModel>();
                }
                
                return new List<WeeklyMenuItemViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception getting weekly menu: {ex.Message}");
            }
            
            return new List<WeeklyMenuItemViewModel>();
        }

        public async Task<Dictionary<DayOfWeek, WeeklyMenuItemViewModel>> GetMenuForWeekAsync(DateTime weekStart, bool isVegetarian)
        {
            var startDate = weekStart;
            var endDate = weekStart.AddDays(6);  // Get entire week
            
            var menuItems = await GetWeeklyMenuAsync(startDate, endDate, isVegetarian);
            var menu = new Dictionary<DayOfWeek, WeeklyMenuItemViewModel>();
            
            foreach (var item in menuItems)
            {
                if (item.MenuDate.DayOfWeek >= DayOfWeek.Monday && item.MenuDate.DayOfWeek <= DayOfWeek.Friday)
                {
                    menu[item.MenuDate.DayOfWeek] = item;
                }
            }
            
            return menu;
        }

        public async Task<List<TicketViewModel>> GetUserTicketsAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Tickets?userId={userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var dtos = await response.Content.ReadFromJsonAsync<List<TicketDTO>>();
                if (dtos == null) return new List<TicketViewModel>();
                
                return dtos.Select(dto => new TicketViewModel
                {
                    Id = dto.Id,
                    UserId = dto.UserId,
                    MenuId = dto.MenuId,
                    CreatedAt = dto.ReservationDate,
                    Status = "Active",
                    Menu = new WeeklyMenuItemViewModel
                    {
                        MenuDate = dto.MenuDate,
                        Price = dto.Price,
                        IsVegetarian = dto.IsVegetarian
                    }
                }).ToList();
            }
            
            return new List<TicketViewModel>();
        }

        public async Task<TicketViewModel> BuyTicketAsync(int userId, int menuId, DateTime mealDate)
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
                var dto = await response.Content.ReadFromJsonAsync<TicketDTO>();
                if (dto == null)
                    return null;
                
                var ticket = new TicketViewModel
                {
                    Id = dto.Id,
                    UserId = dto.UserId,
                    MenuId = dto.MenuId,
                    CreatedAt = dto.ReservationDate,
                    Status = "Active",
                    Menu = new WeeklyMenuItemViewModel
                    {
                        MenuDate = dto.MenuDate,
                        Price = dto.Price,
                        IsVegetarian = dto.IsVegetarian
                    }
                };
                
                return ticket;
            }
            
            return null;
        }
        
        // DTO matching API response structure
        private class TicketDTO
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public int MenuId { get; set; }
            public DateTime MenuDate { get; set; }
            public DateTime ReservationDate { get; set; }
            public decimal Price { get; set; }
            public bool IsVegetarian { get; set; }
        }

        public async Task<bool> CancelTicketAsync(int ticketId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/Tickets/{ticketId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ValidateTicketAsync(int ticketId)
        {
            var response = await _httpClient.PutAsync($"{_baseUrl}/Tickets/{ticketId}/validate", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ProductViewModel>> GetAllProductsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Products");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ProductViewModel>>();
            }
            
            return new List<ProductViewModel>();
        }
    }
}
