using Microsoft.AspNetCore.Mvc;
using refeitorio.Services;
using refeitorio.ViewModels;

namespace refeitorio.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductService _productService;
        private readonly FavoriteService _favoriteService;
        private readonly AuthService _authService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ProductService productService, FavoriteService favoriteService, AuthService authService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _favoriteService = favoriteService;
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? categoryId, string? search = null, decimal? minPrice = null, decimal? maxPrice = null, string? allergens = null, List<string>? excludeAllergens = null)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            
            if (studentId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _authService.GetUserById(studentId.Value);
            ViewBag.StudentName = user?.Name;
            ViewBag.Balance = user?.Balance;
            ViewBag.ProfileImage = user?.ProfileImage;

            search = search?.Trim();
            
            bool hasFilters = !string.IsNullOrWhiteSpace(search) || minPrice.HasValue || maxPrice.HasValue;

            List<ProductViewModel> products;
            if (!hasFilters && !categoryId.HasValue)
            {
                products = await _productService.GetAllProducts(null);
            }
            else if (!hasFilters && categoryId.HasValue)
            {
                products = await _productService.GetAllProducts(categoryId);
            }
            else
            {
                products = await _productService.SearchProducts(search, minPrice, maxPrice, allergens);
                
                if (categoryId.HasValue)
                {
                    products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
                }
            }

            if (excludeAllergens != null && excludeAllergens.Any())
            {
                products = products.Where(p => 
                {
                    if (string.IsNullOrEmpty(p.Allergens)) return true;
                    
                    foreach (var allergen in excludeAllergens)
                    {
                        if (p.Allergens.Contains(allergen, StringComparison.OrdinalIgnoreCase))
                            return false;
                    }
                    return true;
                }).ToList();
            }

            var allAllergens = new List<string> { 
                "Glúten", "Lactose", "Amendoim", "Nozes", "Soja", 
                "Peixe", "Marisco", "Ovos", "Mostarda", "Aipo", 
                "Sésamo", "Sulfitos", "Tremoço", "Moluscos" 
            };
            
            ViewBag.AllAllergens = allAllergens;
            ViewBag.ExcludeAllergens = excludeAllergens ?? new List<string>();

            var categories = await _productService.GetAllCategories();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Search = search;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Allergens = allergens;

            // Obter favoritos do usuário
            var favorites = await _favoriteService.GetUserFavorites(studentId.Value);
            ViewBag.Favorites = favorites.Select(f => f.ProductId).ToList();

            return View(products);
        }
        
        public async Task<IActionResult> Details(int id)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var product = await _productService.GetProductById(id);
            if (product == null) return NotFound();

            var user = await _authService.GetUserById(studentId.Value);
            ViewBag.StudentName = user?.Name;
            ViewBag.Balance = user?.Balance;
            ViewBag.ProfileImage = user?.ProfileImage;
            
            var categories = await _productService.GetAllCategories();
            var category = categories?.FirstOrDefault(c => c.Id == product.CategoryId);
            ViewBag.CategoryName = category?.Name ?? "Sem categoria";
            
            // Verifica se o produto está nos favoritos
            bool isFavorite = await _favoriteService.IsFavorite(studentId.Value, id);
            ViewBag.IsFavorite = isFavorite;

            return View(product);
        }

        [HttpPost]
        public IActionResult Search(string? query = null, int? categoryId = null, string? allergens = null, List<string>? excludeAllergens = null)
        {
            // Redirecionar para o método Index com os parâmetros de pesquisa
            return RedirectToAction("Index", new { categoryId, search = query, allergens, excludeAllergens });
        }
    }
}
