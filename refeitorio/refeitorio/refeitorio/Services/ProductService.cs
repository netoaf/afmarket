using System.Net.Http.Json;
using refeitorio.ViewModels;

namespace refeitorio.Services
{
    public class ProductService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ProductService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7091/api";
            _logger.LogInformation($"ProductService initialized with BaseUrl: {_baseUrl}");
        }

        public async Task<List<ProductViewModel>> GetAllProducts(int? categoryId = null, bool? isVegetarian = null)
        {
            try
            {
                string endpoint = "Products";
                
                if (categoryId.HasValue || isVegetarian.HasValue)
                {
                    endpoint += "?";
                    
                    if (categoryId.HasValue)
                    {
                        endpoint += $"categoryId={categoryId}";
                        if (isVegetarian.HasValue)
                            endpoint += "&";
                    }
                    
                    if (isVegetarian.HasValue)
                        endpoint += $"isVegetarian={isVegetarian.Value}";
                }
                
                var url = $"{_baseUrl}/{endpoint}";
                _logger.LogInformation($"Fetching products from: {url}");
                
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.GetAsync(url);
                
                _logger.LogInformation($"Response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var products = await response.Content.ReadFromJsonAsync<List<ProductViewModel>>();
                    _logger.LogInformation($"Retrieved {products?.Count ?? 0} products");
                    return products ?? new List<ProductViewModel>();
                }
                else
                {
                    _logger.LogWarning($"Failed to fetch products. Status: {response.StatusCode}");
                    return new List<ProductViewModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return new List<ProductViewModel>();
            }
        }

        public async Task<ProductViewModel?> GetProductById(int id)
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync($"{_baseUrl}/Products/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProductViewModel>();
            }
            
            return null;
        }

        public async Task<List<ProductViewModel>> SearchProducts(string? query = null, decimal? minPrice = null, decimal? maxPrice = null, string? allergens = null)
        {
            query = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
            allergens = string.IsNullOrWhiteSpace(allergens) ? null : allergens.Trim();
            
            var queryParams = new List<string>();
            
            if (query != null)
            {
                queryParams.Add($"query={Uri.EscapeDataString(query)}");
            }
            
            if (minPrice.HasValue)
            {
                queryParams.Add($"minPrice={minPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            }
            
            if (maxPrice.HasValue)
            {
                queryParams.Add($"maxPrice={maxPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
            }
            
            if (allergens != null)
            {
                queryParams.Add($"allergens={Uri.EscapeDataString(allergens)}");
            }
            
            string endpoint = queryParams.Count > 0 
                ? $"Products/search?{string.Join("&", queryParams)}"
                : "Products/search";
            
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync($"{_baseUrl}/{endpoint}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<ProductViewModel>>();
                return result ?? new List<ProductViewModel>();
            }
            
            return new List<ProductViewModel>();
        }

        public async Task<List<CategoryViewModel>> GetAllCategories()
        {
            var httpClient = _httpClientFactory.CreateClient("ApiClient");
            var response = await httpClient.GetAsync($"{_baseUrl}/Categories");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<CategoryViewModel>>();
            }
            
            return new List<CategoryViewModel>();
        }
    }
}
