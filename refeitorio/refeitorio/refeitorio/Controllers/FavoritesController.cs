using Microsoft.AspNetCore.Mvc;
using refeitorio.Services;
using refeitorio.ViewModels;

namespace refeitorio.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly FavoriteService _favoriteService;
        private readonly AuthService _authService;
        private readonly ProductService _productService;

        public FavoritesController(FavoriteService favoriteService, AuthService authService, ProductService productService)
        {
            _favoriteService = favoriteService;
            _authService = authService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
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

            // Obter favoritos do usuário através da API
            var favorites = await _favoriteService.GetUserFavorites(studentId.Value);
            var products = new List<ProductViewModel>();
            
            // Para cada favorito, obter os detalhes do produto
            foreach (var favorite in favorites)
            {
                var product = await _productService.GetProductById(favorite.ProductId);
                if (product != null)
                {
                    products.Add(product);
                }
            }

            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int productId)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            // Pedidos enviados via Fetch trazem este header, permite responder em JSON sem redirecionar
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            if (studentId == null)
                return isAjax ? Json(new { success = false }) : RedirectToAction("Login", "Auth");

            // Verificar se o produto já é um favorito
            bool isFavorite = await _favoriteService.IsFavorite(studentId.Value, productId);
            
            if (isFavorite)
            {
                // Remover dos favoritos
                await _favoriteService.RemoveFavorite(studentId.Value, productId);
                isFavorite = false;
            }
            else
            {
                // Adicionar aos favoritos
                await _favoriteService.AddFavorite(studentId.Value, productId);
                isFavorite = true;
            }
            
            // Obter contagem atualizada de favoritos
            var favorites = await _favoriteService.GetUserFavorites(studentId.Value);
            int favoritesCount = favorites?.Count ?? 0;
            
            // Resposta AJAX devolve estado atualizado; fallback redireciona para fluxo tradicional
            return isAjax 
                ? Json(new { success = true, isFavorite, favoritesCount })
                : RedirectToAction("Index", "Products");
        }
    }
}
