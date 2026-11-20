using Microsoft.AspNetCore.Mvc;
using refeitorio.Services;
using refeitorio.ViewModels;

namespace refeitorio.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly ProductService _productService;
        private readonly AuthService _authService;
        private readonly OrderService _orderService;

        public CartController(CartService cartService, ProductService productService, AuthService authService, OrderService orderService)
        {
            _cartService = cartService;
            _productService = productService;
            _authService = authService;
            _orderService = orderService;
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

            var cartResponse = await _cartService.GetCart(studentId.Value);
            ViewBag.Total = cartResponse.Items?.Sum(c => c.UnitPrice * c.Quantity) ?? 0;
            ViewBag.HasSufficientBalance = cartResponse.HasSufficientBalance;

            // Buscar histórico de pedidos do usuário (mais recentes primeiro)
            var orders = await _orderService.GetUserOrdersAsync(studentId.Value);
            ViewBag.RecentOrders = orders?.OrderByDescending(o => o.OrderDate).ToList() ?? new List<OrderDTO>();

            return View(cartResponse.Items ?? new List<CartItemViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            // Se o pedido vier via Fetch devolvemos JSON em vez de redirecionar
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            if (studentId == null)
                return isAjax ? Json(new { success = false }) : RedirectToAction("Login", "Auth");

            // Adicionar ao carrinho via API
            var response = await _cartService.AddToCart(studentId.Value, productId, quantity);
            
            if (response == null)
                return isAjax ? Json(new { success = false }) : RedirectToAction("Index", "Products");

            // Obter o carrinho atualizado
            var cartResponse = await _cartService.GetCart(studentId.Value);
            var cartItems = cartResponse.Items ?? new List<CartItemViewModel>();
            
            return isAjax
                ? Json(new { 
                    success = true,
                    cartCount = cartItems?.Sum(c => c.Quantity) ?? 0,
                    cartTotal = cartItems?.Sum(c => c.UnitPrice * c.Quantity) ?? 0,
                    hasSufficientBalance = response.HasSufficientBalance
                })
                : RedirectToAction("Index", "Products");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            if (studentId == null)
                return isAjax ? Json(new { success = false, message = "Sessão expirada" }) : RedirectToAction("Login", "Auth");

            // Obter o carrinho para encontrar o cartItemId
            var cartResponse = await _cartService.GetCart(studentId.Value);
            var currentItem = cartResponse.Items?.FirstOrDefault(i => i.ProductId == productId);
            
            if (currentItem == null)
            {
                if (isAjax) return Json(new { success = false, message = "Produto não encontrado no carrinho." });
                TempData["Error"] = "Produto não encontrado no carrinho.";
                return RedirectToAction("Index");
            }
            
            int cartItemId = currentItem.Id;
            int newQuantity = quantity;

            // Atualizar a quantidade via API
            UpdateCartResponse response;
            
            if (newQuantity <= 0)
            {
                // Remover o item do carrinho
                response = await _cartService.RemoveFromCart(cartItemId, studentId.Value);
            }
            else
            {
                // Atualizar a quantidade
                response = await _cartService.UpdateQuantity(cartItemId, newQuantity, studentId.Value);
            }

            if (response == null)
            {
                if (isAjax) return Json(new { success = false, message = "Erro ao atualizar quantidade." });
                TempData["Error"] = "Erro ao atualizar quantidade.";
                return RedirectToAction("Index");
            }

            // Obter o carrinho atualizado para exibir totais e quantidades
            var updatedCart = await _cartService.GetCart(studentId.Value);
            var items = updatedCart.Items ?? new List<CartItemViewModel>();
            var updatedItem = items.FirstOrDefault(i => i.Id == cartItemId);
            
            // Obter usuário para saldo atualizado
            var user = await _authService.GetUserById(studentId.Value);

            if (isAjax)
            {
                return Json(new
                {
                    success = true,
                    cartCount = items.Sum(c => c.Quantity),
                    cartTotal = items.Sum(c => c.UnitPrice * c.Quantity),
                    itemQuantity = updatedItem?.Quantity ?? 0,
                    itemSubtotal = updatedItem != null ? updatedItem.UnitPrice * updatedItem.Quantity : 0,
                    itemRemoved = updatedItem == null,
                    balance = user?.Balance ?? 0,
                    hasSufficientBalance = updatedCart.HasSufficientBalance
                });
            }

            return RedirectToAction("Index");
        }

        public class UpdateQuantityRequest
        {
            public int productId { get; set; }
            public int cartItemId { get; set; }
            public int quantity { get; set; }
            public int change { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            // Flag usada pelas ações do carrinho (botões AJAX na UI)
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            if (studentId == null)
                return isAjax ? Json(new { success = false, message = "Sessão expirada" }) : RedirectToAction("Login", "Auth");

            // Remover o item do carrinho via API
            var response = await _cartService.RemoveFromCart(cartItemId, studentId.Value);

            if (response == null)
                return isAjax ? Json(new { success = false, message = "Erro ao remover item" }) : RedirectToAction("Index");
            
            // Obter o carrinho atualizado
            var cartResponse = await _cartService.GetCart(studentId.Value);
            var cartItems = cartResponse.Items ?? new List<CartItemViewModel>();
            
            return isAjax
                ? Json(new { 
                    success = true,
                    message = "Produto removido do carrinho",
                    cartCount = cartItems.Sum(c => c.Quantity),
                    cartTotal = cartItems.Sum(c => c.UnitPrice * c.Quantity),
                    removeElement = $"#cart-row-{cartItemId}",
                    reload = !cartItems.Any(),
                    hasSufficientBalance = response.HasSufficientBalance
                })
                : RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            // Checkout suporta AJAX (Fetch) para evitar recarregar a página
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            if (studentId == null)
                return isAjax ? Json(new { success = false, message = "Sessão expirada" }) : RedirectToAction("Login", "Auth");

            // Verificar o carrinho e o saldo via API
            var cartResponse = await _cartService.GetCart(studentId.Value);
            
            if (cartResponse.Items == null || cartResponse.Items.Count == 0)
            {
                var emptyMsg = "Carrinho vazio!";
                if (isAjax) return Json(new { success = false, message = emptyMsg });
                TempData["Error"] = emptyMsg;
                return RedirectToAction("Index");
            }
            
            if (!cartResponse.HasSufficientBalance)
            {
                var msg = "Saldo insuficiente!";
                if (isAjax) return Json(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToAction("Index");
            }

            // Criar o pedido via API
            var orderDto = new CreateOrderDTO { UserId = studentId.Value };
            // A lógica de negócio acontece na API: verificar estoque, baixa em estoque, desconto em saldo, etc.
            var order = await _orderService.CreateFromCartAsync(orderDto.UserId);

            if (order == null)
            {
                var errorMsg = "Erro ao processar o pedido. Verifique o estoque e saldo.";
                if (isAjax) return Json(new { success = false, message = errorMsg });
                TempData["Error"] = errorMsg;
                return RedirectToAction("Index");
            }

            // Obter o saldo atualizado do usuário
            var user = await _authService.GetUserById(studentId.Value);
            var successMsg = $"Compra realizada com sucesso! Novo saldo: €{user?.Balance ?? 0:F2}";
            
            if (isAjax)
                return Json(new { 
                    success = true,
                    message = successMsg,
                    balance = user?.Balance ?? 0,
                    cartCount = 0,
                    redirect = Url.Action("Index", "Products")
                });
            
            TempData["Success"] = successMsg;
            return RedirectToAction("Index", "Products");
        }

    }
}