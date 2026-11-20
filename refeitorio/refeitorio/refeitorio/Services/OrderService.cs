using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace refeitorio.Services
{
    public class OrderService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public OrderService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7091/api";
        }

        public async Task<List<OrderDTO>> GetAllAsync(int? userId = null, string status = null)
        {
            string endpoint = "Orders";
            bool hasParam = false;
            
            if (userId.HasValue)
            {
                endpoint += $"?userId={userId.Value}";
                hasParam = true;
            }
            
            if (!string.IsNullOrEmpty(status))
            {
                endpoint += hasParam ? "&" : "?";
                endpoint += $"status={status}";
            }
            
            var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<OrderDTO>>();
            }
            
            return new List<OrderDTO>();
        }

        public async Task<OrderDTO> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Orders/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OrderDTO>();
            }
            
            return null;
        }

        public async Task<List<OrderDTO>> GetUserOrdersAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Orders/user/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<OrderDTO>>();
            }
            
            return new List<OrderDTO>();
        }

        public async Task<OrderDTO> CreateFromCartAsync(int userId)
        {
            var orderDto = new CreateOrderDTO { UserId = userId };
            var content = new StringContent(JsonSerializer.Serialize(orderDto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/Orders", content);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<OrderDTO>();
            }
            
            return null;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status, int? staffId = null)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(staffId), Encoding.UTF8, "application/json");
                var url = $"{_baseUrl}/Orders/{id}/status/{status}";
                var response = await _httpClient.PutAsync(url, content);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<OrderDTO>> GetOrdersByStatusAsync(string status)
        {
            return await GetAllAsync(null, status);
        }

        public async Task<List<OrderDTO>> GetAllOrdersAsync()
        {
            return await GetAllAsync(null, null);
        }

        public async Task<bool> ApproveOrderAsync(int orderId, int staffId)
        {
            return await UpdateStatusAsync(orderId, "Delivered", staffId);
        }

        public async Task<bool> RejectOrderAsync(int orderId)
        {
            return await UpdateStatusAsync(orderId, "Canceled", null);
        }
        
        public async Task<bool> ApproveToReadyAsync(int orderId)
        {
            return await UpdateStatusAsync(orderId, "Ready", null);
        }
    }

    public class CreateOrderDTO
    {
        public int UserId { get; set; }
    }

    public class OrderDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public List<OrderItemDTO> Items { get; set; }
    }

    public class OrderItemDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
