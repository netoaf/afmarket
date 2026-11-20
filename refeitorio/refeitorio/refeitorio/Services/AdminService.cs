using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using refeitorio.ViewModels;

namespace refeitorio.Services
{
    public class AdminService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl;

        public AdminService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7091/api";
        }

        #region User Management
        public async Task<List<UserViewModel>> GetAllUsersAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync($"{_baseUrl}/Users");
            
            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<List<UserViewModel>>();
                return users ?? new List<UserViewModel>();
            }
            
            return new List<UserViewModel>();
        }

        public async Task<UserViewModel> GetUserByIdAsync(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync($"{_baseUrl}/Users/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserViewModel>();
            }
            
            return null;
        }

        public async Task<UserViewModel> CreateUserAsync(UserViewModel user)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{_baseUrl}/Users", content);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserViewModel>();
            }
            
            return null;
        }

        public async Task<bool> UpdateUserAsync(UserViewModel user)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"{_baseUrl}/Users/{user.Id}", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.DeleteAsync($"{_baseUrl}/Users/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddUserBalanceAsync(int userId, decimal amount)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(JsonSerializer.Serialize(amount), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{_baseUrl}/Users/{userId}/addbalance", content);
            
            return response.IsSuccessStatusCode;
        }
        #endregion

        #region Category Management
        public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync($"{_baseUrl}/Categories");
            
            if (response.IsSuccessStatusCode)
            {
                var categories = await response.Content.ReadFromJsonAsync<List<CategoryViewModel>>();
                return categories ?? new List<CategoryViewModel>();
            }
            
            return new List<CategoryViewModel>();
        }

        public async Task<CategoryViewModel> GetCategoryByIdAsync(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync($"{_baseUrl}/Categories/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CategoryViewModel>();
            }
            
            return null;
        }

        public async Task<CategoryViewModel> CreateCategoryAsync(CategoryViewModel category)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(JsonSerializer.Serialize(category), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{_baseUrl}/Categories", content);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<CategoryViewModel>();
            }
            
            return null;
        }

        public async Task<bool> UpdateCategoryAsync(CategoryViewModel category)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(JsonSerializer.Serialize(category), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"{_baseUrl}/Categories/{category.Id}", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.DeleteAsync($"{_baseUrl}/Categories/{id}");
            return response.IsSuccessStatusCode;
        }
        #endregion

        #region Product Management
        public async Task<List<ProductViewModel>> GetAllProductsAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync($"{_baseUrl}/Products");
            
            if (response.IsSuccessStatusCode)
            {
                var products = await response.Content.ReadFromJsonAsync<List<ProductViewModel>>();
                return products ?? new List<ProductViewModel>();
            }
            
            return new List<ProductViewModel>();
        }

        public async Task<ProductViewModel> GetProductByIdAsync(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync($"{_baseUrl}/Products/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProductViewModel>();
            }
            
            return null;
        }

        public async Task<ProductViewModel> CreateProductAsync(ProductViewModel product)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{_baseUrl}/Products", content);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProductViewModel>();
            }
            
            return null;
        }

        public async Task<bool> UpdateProductAsync(ProductViewModel product)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"{_baseUrl}/Products/{product.Id}", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.DeleteAsync($"{_baseUrl}/Products/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateProductStockAsync(int id, int quantity)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(JsonSerializer.Serialize(quantity), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"{_baseUrl}/Products/{id}/stock", content);
            
            return response.IsSuccessStatusCode;
        }
        #endregion

        #region Weekly Menu Management
        public async Task<List<WeeklyMenuItemViewModel>> GetAllWeeklyMenuItemsAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync($"{_baseUrl}/WeeklyMenu");
            
            if (response.IsSuccessStatusCode)
            {
                var menuItems = await response.Content.ReadFromJsonAsync<List<WeeklyMenuItemViewModel>>();
                return menuItems ?? new List<WeeklyMenuItemViewModel>();
            }
            
            return new List<WeeklyMenuItemViewModel>();
        }

        public async Task<WeeklyMenuItemViewModel> GetWeeklyMenuItemByIdAsync(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync($"{_baseUrl}/WeeklyMenu/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WeeklyMenuItemViewModel>();
            }
            
            return null;
        }

        public async Task<WeeklyMenuItemViewModel> CreateWeeklyMenuItemAsync(WeeklyMenuItemViewModel menuItem)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(JsonSerializer.Serialize(menuItem), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync($"{_baseUrl}/WeeklyMenu", content);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WeeklyMenuItemViewModel>();
            }
            
            return null;
        }

        public async Task<bool> UpdateWeeklyMenuItemAsync(WeeklyMenuItemViewModel menuItem)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var content = new StringContent(JsonSerializer.Serialize(menuItem), Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"{_baseUrl}/WeeklyMenu/{menuItem.Id}", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteWeeklyMenuItemAsync(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.DeleteAsync($"{_baseUrl}/WeeklyMenu/{id}");
            return response.IsSuccessStatusCode;
        }
        #endregion
    }
}
