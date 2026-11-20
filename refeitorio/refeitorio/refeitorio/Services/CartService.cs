using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using refeitorio.ViewModels;

namespace refeitorio.Services
{
    public class CartService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public CartService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7091/api";
        }

        public async Task<CartResponse> GetCart(int userId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Cart/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var cart = await response.Content.ReadFromJsonAsync<CartResponse>();
                return cart ?? new CartResponse { Items = new List<CartItemViewModel>(), HasSufficientBalance = false };
            }
            
            return new CartResponse { Items = new List<CartItemViewModel>(), HasSufficientBalance = false };
        }

        public async Task<AddToCartResponse> AddToCart(int userId, int productId, int quantity = 1)
        {
            var cartItem = new
            {
                UserId = userId,
                ProductId = productId,
                Quantity = quantity
            };

            var content = new StringContent(JsonSerializer.Serialize(cartItem), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/Cart", content);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AddToCartResponse>();
                return result ?? new AddToCartResponse { Item = null, HasSufficientBalance = false };
            }
            
            return new AddToCartResponse { Item = null, HasSufficientBalance = false };
        }

        public async Task<UpdateCartResponse> UpdateQuantity(int cartItemId, int quantity, int userId)
        {
            var response = await _httpClient.PutAsync($"{_baseUrl}/Cart/{cartItemId}/quantity/{quantity}/user/{userId}", null);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UpdateCartResponse>();
                return result ?? new UpdateCartResponse { Items = new List<CartItemViewModel>(), HasSufficientBalance = false };
            }
            
            return new UpdateCartResponse { Items = new List<CartItemViewModel>(), HasSufficientBalance = false };
        }

        public async Task<UpdateCartResponse> RemoveFromCart(int cartItemId, int userId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/Cart/{cartItemId}/user/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UpdateCartResponse>();
                return result ?? new UpdateCartResponse { Items = new List<CartItemViewModel>(), HasSufficientBalance = false };
            }
            
            return new UpdateCartResponse { Items = new List<CartItemViewModel>(), HasSufficientBalance = false };
        }

        public async Task<bool> ClearCart(int userId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/Cart/user/{userId}");
            return response.IsSuccessStatusCode;
        }
    }

    // Classes para as respostas da API
    public class CartResponse
    {
        public List<CartItemViewModel> Items { get; set; }
        public bool HasSufficientBalance { get; set; }
    }

    public class AddToCartResponse
    {
        public CartItemViewModel Item { get; set; }
        public bool HasSufficientBalance { get; set; }
    }

    public class UpdateCartResponse
    {
        public List<CartItemViewModel> Items { get; set; }
        public bool HasSufficientBalance { get; set; }
    }
}
