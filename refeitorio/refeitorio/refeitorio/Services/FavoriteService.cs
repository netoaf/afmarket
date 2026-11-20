using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace refeitorio.Services
{
    public class FavoriteService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public FavoriteService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7091/api";
        }

        public async Task<List<Favorite>> GetUserFavorites(int userId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/Favorites/user/{userId}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<Favorite>>();
            }
            
            return new List<Favorite>();
        }

        public async Task<Favorite> AddFavorite(int userId, int productId)
        {
            var favoriteData = new
            {
                UserId = userId,
                ProductId = productId
            };

            var content = new StringContent(JsonSerializer.Serialize(favoriteData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/Favorites", content);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Favorite>();
            }
            
            return null;
        }

        public async Task<bool> RemoveFavorite(int userId, int productId)
        {
            var response = await _httpClient.DeleteAsync($"{_baseUrl}/Favorites/user/{userId}/product/{productId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> IsFavorite(int userId, int productId)
        {
            var favorites = await GetUserFavorites(userId);
            return favorites.Any(f => f.ProductId == productId);
        }
    }

    // Classe para representar um favorito
    public class Favorite
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
