using Microsoft.AspNetCore.Mvc;
using refeitorio.Services;
using refeitorio.ViewModels;

namespace refeitorio.Controllers
{
    public class StaffController : Controller
    {
        private readonly OrderService _orderService;
        private readonly AuthService _authService;
        private readonly AdminService _adminService;

        public StaffController(OrderService orderService, AuthService authService, AdminService adminService)
        {
            _orderService = orderService;
            _authService = authService;
            _adminService = adminService;
        }

        private async Task<bool> IsStaffOrAdmin()
        {
            var userId = HttpContext.Session.GetInt32("StudentId");
            if (userId == null) return false;

            var user = await _authService.GetUserById(userId.Value);
            return user?.UserType == "Staff" || user?.UserType == "Admin";
        }

        public async Task<IActionResult> Orders()
        {
            var staffId = HttpContext.Session.GetInt32("StudentId");
            if (staffId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _authService.GetUserById(staffId.Value);
            if (user == null || !await IsStaffOrAdmin())
            {
                TempData["Error"] = "Acesso negado. Apenas funcionários e administradores podem acessar esta página.";
                return RedirectToAction("Index", "Products");
            }

            ViewBag.StaffName = user.Name;
            ViewBag.Balance = user.Balance;
            ViewBag.ProfileImage = user.ProfileImage;

            // Obter pedidos pendentes e prontos (Pending + Ready)
            var allOrders = await _orderService.GetAllOrdersAsync();
            var activeOrders = allOrders.Where(o => o.Status == "Pending" || o.Status == "Ready").ToList();
            
            return View(activeOrders);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveOrder(int orderId)
        {
            var staffId = HttpContext.Session.GetInt32("StudentId");
            if (staffId == null)
            {
                return Json(new { success = false, message = "Sessão expirada" });
            }

            if (!await IsStaffOrAdmin())
            {
                return Json(new { success = false, message = "Acesso negado" });
            }

            // Transição: Pending -> Ready
            var success = await _orderService.ApproveToReadyAsync(orderId);
            
            if (success)
            {
                return Json(new { success = true, message = "Pedido aprovado e marcado como pronto!" });
            }

            return Json(new { success = false, message = "Erro ao aprovar pedido" });
        }

        [HttpPost]
        public async Task<IActionResult> DeliverOrder(int orderId)
        {
            var staffId = HttpContext.Session.GetInt32("StudentId");
            if (staffId == null)
            {
                return Json(new { success = false, message = "Sessão expirada" });
            }

            if (!await IsStaffOrAdmin())
            {
                return Json(new { success = false, message = "Acesso negado" });
            }

            // Transição: Ready -> Delivered
            var success = await _orderService.ApproveOrderAsync(orderId, staffId.Value);
            
            if (success)
            {
                return Json(new { success = true, message = "Pedido entregue com sucesso!" });
            }

            return Json(new { success = false, message = "Erro ao entregar pedido" });
        }

        [HttpPost]
        public async Task<IActionResult> RejectOrder(int orderId)
        {
            if (!await IsStaffOrAdmin())
            {
                return Json(new { success = false, message = "Acesso negado" });
            }

            // Transição: Pending -> Canceled (ou Ready -> Canceled)
            var success = await _orderService.RejectOrderAsync(orderId);
            
            if (success)
            {
                return Json(new { success = true, message = "Pedido cancelado. Saldo devolvido ao utilizador." });
            }

            return Json(new { success = false, message = "Erro ao cancelar pedido" });
        }

        public async Task<IActionResult> OrderHistory()
        {
            var staffId = HttpContext.Session.GetInt32("StudentId");
            if (staffId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!await IsStaffOrAdmin())
            {
                TempData["Error"] = "Acesso negado. Apenas funcionários e administradores podem acessar esta página.";
                return RedirectToAction("Index", "Products");
            }

            var user = await _authService.GetUserById(staffId.Value);
            ViewBag.StaffName = user.Name;
            ViewBag.Balance = user.Balance;
            ViewBag.ProfileImage = user.ProfileImage;

            // Obter todos os pedidos (exceto pendentes)
            var allOrders = await _orderService.GetAllOrdersAsync();
            var completedOrders = allOrders.Where(o => o.Status != "Pending").OrderByDescending(o => o.OrderDate).ToList();
            
            return View(completedOrders);
        }

        // GESTÃO DE PRODUTOS (Staff e Admin)
        public async Task<IActionResult> Products()
        {
            if (!await IsStaffOrAdmin())
            {
                TempData["Error"] = "Acesso negado.";
                return RedirectToAction("Index", "Products");
            }

            var user = await _authService.GetUserById(HttpContext.Session.GetInt32("StudentId").Value);
            ViewBag.StaffName = user.Name;
            ViewBag.Balance = user.Balance;
            ViewBag.ProfileImage = user.ProfileImage;

            var products = await _adminService.GetAllProductsAsync();
            var categories = await _adminService.GetAllCategoriesAsync();
            ViewBag.CategoryNames = categories.ToDictionary(c => c.Id, c => c.Name);
            ViewBag.Categories = categories;
            
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStock(int productId, int newStock)
        {
            if (!await IsStaffOrAdmin())
            {
                return Json(new { success = false, message = "Acesso negado" });
            }

            var product = await _adminService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Produto não encontrado" });
            }

            product.Stock = newStock;
            var success = await _adminService.UpdateProductAsync(product);

            if (success)
            {
                return Json(new { success = true, message = "Stock atualizado com sucesso!" });
            }

            return Json(new { success = false, message = "Erro ao atualizar stock" });
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            if (!await IsStaffOrAdmin())
            {
                TempData["Error"] = "Acesso negado.";
                return RedirectToAction("Index", "Products");
            }

            ViewBag.Categories = await _adminService.GetAllCategoriesAsync();
            var product = await _adminService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            var user = await _authService.GetUserById(HttpContext.Session.GetInt32("StudentId").Value);
            ViewBag.StaffName = user.Name;
            ViewBag.Balance = user.Balance;
            ViewBag.ProfileImage = user.ProfileImage;

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductViewModel product)
        {
            if (!await IsStaffOrAdmin())
            {
                TempData["Error"] = "Acesso negado.";
                return RedirectToAction("Index", "Products");
            }

            var success = await _adminService.UpdateProductAsync(product);
            
            if (!success)
            {
                ModelState.AddModelError("", "Erro ao atualizar produto");
                ViewBag.Categories = await _adminService.GetAllCategoriesAsync();
                return View(product);
            }
            
            return RedirectToAction("Products");
        }

        // GESTÃO DE CATEGORIAS (Staff e Admin)
        public async Task<IActionResult> Categories()
        {
            if (!await IsStaffOrAdmin())
            {
                TempData["Error"] = "Acesso negado.";
                return RedirectToAction("Index", "Products");
            }

            var user = await _authService.GetUserById(HttpContext.Session.GetInt32("StudentId").Value);
            ViewBag.StaffName = user.Name;
            ViewBag.Balance = user.Balance;
            ViewBag.ProfileImage = user.ProfileImage;

            var categories = await _adminService.GetAllCategoriesAsync();
            return View(categories);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCategory()
        {
            if (!await IsStaffOrAdmin())
            {
                TempData["Error"] = "Acesso negado.";
                return RedirectToAction("Index", "Products");
            }

            var user = await _authService.GetUserById(HttpContext.Session.GetInt32("StudentId").Value);
            ViewBag.StaffName = user.Name;
            ViewBag.Balance = user.Balance;
            ViewBag.ProfileImage = user.ProfileImage;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryViewModel category)
        {
            if (!await IsStaffOrAdmin())
            {
                return BadRequest(new { success = false, message = "Acesso negado" });
            }

            var createdCategory = await _adminService.CreateCategoryAsync(category);
            
            if (createdCategory == null)
            {
                return BadRequest(new { success = false, message = "Erro ao criar categoria" });
            }
            
            return Json(new { success = true, message = "Categoria criada com sucesso!" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            if (!await IsStaffOrAdmin())
            {
                return Json(new { success = false, message = "Acesso negado" });
            }

            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var success = await _adminService.DeleteCategoryAsync(id);
            
            if (success)
            {
                if (isAjax)
                    return Json(new { success = true, message = "Categoria eliminada com sucesso!" });
            }
            else if (isAjax)
            {
                return Json(new { success = false, message = "Categoria não encontrada ou erro ao eliminar!" });
            }
            
            return RedirectToAction("Categories");
        }
    }
}
