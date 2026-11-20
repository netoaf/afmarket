using Microsoft.AspNetCore.Mvc;
using refeitorio.ViewModels;
using refeitorio.Services;

namespace refeitorio.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;
        private readonly AuthService _authService;

        public AdminController(AdminService adminService, AuthService authService)
        {
            _adminService = adminService;
            _authService = authService;
        }

        private async Task<bool> IsAdmin()
        {
            var userId = HttpContext.Session.GetInt32("StudentId");
            if (userId == null) return false;

            var user = await _authService.GetUserById(userId.Value);
            return user?.UserType == "Admin";
        }

        public async Task<IActionResult> Index()
        {
            if (!await IsAdmin())
            {
                TempData["Error"] = "Acesso negado. Apenas administradores podem acessar esta área.";
                return RedirectToAction("Index", "Home");
            }
            
            var userId = HttpContext.Session.GetInt32("StudentId");
            var currentUser = await _authService.GetUserById(userId.Value);
            ViewBag.Balance = currentUser?.Balance;
            ViewBag.ProfileImage = currentUser?.ProfileImage;
            
            return View();
        }

        // Dicionário com os dias da semana (só dias úteis)
        private static Dictionary<int, string> DaysOfWeekDict => new()
        {
            [(int)DayOfWeek.Monday] = "Segunda-feira",
            [(int)DayOfWeek.Tuesday] = "Terça-feira",
            [(int)DayOfWeek.Wednesday] = "Quarta-feira",
            [(int)DayOfWeek.Thursday] = "Quinta-feira",
            [(int)DayOfWeek.Friday] = "Sexta-feira"
        };

        public async Task<IActionResult> Students()
        {
            if (!await IsAdmin())
            {
                TempData["Error"] = "Acesso negado. Apenas administradores podem acessar esta área.";
                return RedirectToAction("Index", "Home");
            }
            
            var userId = HttpContext.Session.GetInt32("StudentId");
            var currentUser = await _authService.GetUserById(userId.Value);
            ViewBag.Balance = currentUser?.Balance;
            ViewBag.ProfileImage = currentUser?.ProfileImage;
            
            var users = await _adminService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public IActionResult CreateStudent()
        {
            return View(new UserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent(UserViewModel user, string Balance)
        {
            var createdUser = await _adminService.CreateUserAsync(user);
            
            if (createdUser == null)
            {
                ModelState.AddModelError("", "Erro ao criar usuário");
                return View(user);
            }
            
            return RedirectToAction("Students");
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditStudent(UserViewModel user, string Balance)
        {
            var success = await _adminService.UpdateUserAsync(user);
            
            if (!success)
            {
                ModelState.AddModelError("", "Erro ao atualizar usuário");
                return View(user);
            }
            
            return RedirectToAction("Students");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            // Ações do backoffice podem ser acionadas via AJAX para remoção inline
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            var success = await _adminService.DeleteUserAsync(id);
            
            if (success)
            {
                if (isAjax)
                    return Json(new { success = true, message = "Usuário eliminado com sucesso!" });
            }
            else if (isAjax)
            {
                return Json(new { success = false, message = "Usuário não encontrado ou erro ao eliminar!" });
            }
            
            return RedirectToAction("Students");
        }

        public async Task<IActionResult> Products()
        {
            var userId = HttpContext.Session.GetInt32("StudentId");
            var currentUser = await _authService.GetUserById(userId.Value);
            ViewBag.Balance = currentUser?.Balance;
            ViewBag.ProfileImage = currentUser?.ProfileImage;
            
            var products = await _adminService.GetAllProductsAsync();
            var categories = await _adminService.GetAllCategoriesAsync();
            
            // Criar dicionário de IDs de categorias para nomes
            ViewBag.CategoryNames = categories.ToDictionary(c => c.Id, c => c.Name);
            
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewBag.Categories = await _adminService.GetAllCategoriesAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductViewModel product)
        {
            var createdProduct = await _adminService.CreateProductAsync(product);
            
            if (createdProduct == null)
            {
                ModelState.AddModelError("", "Erro ao criar produto");
                ViewBag.Categories = await _adminService.GetAllCategoriesAsync();
                return View(product);
            }
            
            return RedirectToAction("Products");
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            ViewBag.Categories = await _adminService.GetAllCategoriesAsync();
            var product = await _adminService.GetProductByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductViewModel product)
        {
            var success = await _adminService.UpdateProductAsync(product);
            
            if (!success)
            {
                ModelState.AddModelError("", "Erro ao atualizar produto");
                ViewBag.Categories = await _adminService.GetAllCategoriesAsync();
                return View(product);
            }
            
            return RedirectToAction("Products");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            var success = await _adminService.DeleteProductAsync(id);
            
            if (success)
            {
                if (isAjax)
                    return Json(new { success = true, message = "Produto eliminado com sucesso!" });
            }
            else if (isAjax)
            {
                return Json(new { success = false, message = "Produto não encontrado ou erro ao eliminar!" });
            }
            
            return RedirectToAction("Products");
        }

        public async Task<IActionResult> Categories()
        {
            var userId = HttpContext.Session.GetInt32("StudentId");
            var currentUser = await _authService.GetUserById(userId.Value);
            ViewBag.Balance = currentUser?.Balance;
            ViewBag.ProfileImage = currentUser?.ProfileImage;
            
            var categories = await _adminService.GetAllCategoriesAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryViewModel category)
        {
            var createdCategory = await _adminService.CreateCategoryAsync(category);
            
            if (createdCategory == null)
            {
                ModelState.AddModelError("", "Erro ao criar categoria");
                return View(category);
            }
            
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            // Permite apagar categorias via Fetch sem recarregar a página
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

        public async Task<IActionResult> WeeklyMenu()
        {
            if (!await IsAdmin())
            {
                TempData["Error"] = "Acesso negado. Apenas administradores podem acessar esta área.";
                return RedirectToAction("Index", "Home");
            }
            
            var userId = HttpContext.Session.GetInt32("StudentId");
            var currentUser = await _authService.GetUserById(userId.Value);
            ViewBag.Balance = currentUser?.Balance;
            ViewBag.ProfileImage = currentUser?.ProfileImage;
            
            var menuItems = await _adminService.GetAllWeeklyMenuItemsAsync();
            var allProducts = await _adminService.GetAllProductsAsync();
            
            menuItems = menuItems
                .OrderByDescending(m => m.MenuDate)
                .ThenBy(m => m.IsVegetarian)
                .ToList();
            
            // Criar dicionário de produtos para cada item do menu
            var productsDict = new Dictionary<int, List<ProductViewModel>>();
            foreach (var menuItem in menuItems)
            {
                productsDict[menuItem.Id] = allProducts
                    .Where(p => menuItem.ProductIds.Contains(p.Id))
                    .ToList();
            }
            
            ViewBag.ProductsDict = productsDict;
            
            return View(menuItems);
        }

        [HttpGet]
        public async Task<IActionResult> CreateWeeklyMenuItem()
        {
            var products = await _adminService.GetAllProductsAsync();
            ViewBag.Products = products
                .Where(p => p.IsAvailable)
                .OrderBy(p => p.CategoryId)
                .ThenBy(p => p.Name)
                .ToList();

            ViewBag.DaysOfWeek = DaysOfWeekDict;

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CreateWeeklyMenuItem(WeeklyMenuItemViewModel menuItem, int[] productIds, string Price, DateTime MenuDate, int MaxTickets = 150)
        {
            menuItem.MenuDate = MenuDate.Date;
            
            // Validação: não permitir menus ao fim de semana
            if (menuItem.MenuDate.DayOfWeek == DayOfWeek.Saturday || menuItem.MenuDate.DayOfWeek == DayOfWeek.Sunday)
            {
                TempData["Error"] = "Não é possível criar menus ao fim de semana!";
                
                var products = await _adminService.GetAllProductsAsync();
                ViewBag.Products = products
                    .Where(p => p.IsAvailable)
                    .OrderBy(p => p.CategoryId)
                    .ThenBy(p => p.Name)
                    .ToList();
                    
                ViewBag.DaysOfWeek = DaysOfWeekDict;

                return View(menuItem);
            }

            // Verificar se já existe um menu para essa data e tipo
            var allMenuItems = await _adminService.GetAllWeeklyMenuItemsAsync();
            var existingMenu = allMenuItems.FirstOrDefault(m =>
                m.MenuDate.Date == menuItem.MenuDate.Date &&
                m.IsVegetarian == menuItem.IsVegetarian);

            if (existingMenu != null)
            {
                TempData["Error"] = $"Já existe um menu {(menuItem.IsVegetarian ? "vegetariano" : "regular")} para {menuItem.MenuDate:dd/MM/yyyy}!";

                var products = await _adminService.GetAllProductsAsync();
                ViewBag.Products = products
                    .Where(p => p.IsAvailable)
                    .OrderBy(p => p.CategoryId)
                    .ThenBy(p => p.Name)
                    .ToList();

                ViewBag.DaysOfWeek = DaysOfWeekDict;
                return View(menuItem);
            }

            // Configurar preço do menu
            if (!string.IsNullOrWhiteSpace(Price) && TryParseDecimal(Price, out var parsedMenuPrice))
            {
                menuItem.Price = parsedMenuPrice;
            }
            
            // Criar o menu via API
            var createdMenu = await _adminService.CreateWeeklyMenuItemAsync(menuItem);
            
            if (createdMenu == null)
            {
                ModelState.AddModelError("", "Erro ao criar menu semanal");
                var products = await _adminService.GetAllProductsAsync();
                ViewBag.Products = products
                    .Where(p => p.IsAvailable)
                    .OrderBy(p => p.CategoryId)
                    .ThenBy(p => p.Name)
                    .ToList();
                    
                ViewBag.DaysOfWeek = DaysOfWeekDict;
                return View(menuItem);
            }

            TempData["Success"] = "Menu criado com sucesso!";
            return RedirectToAction("WeeklyMenu");
        }


        [HttpGet]
        public async Task<IActionResult> EditWeeklyMenuItem(int id)
        {
            var menuItem = await _adminService.GetWeeklyMenuItemByIdAsync(id);
            if (menuItem == null) return NotFound();

            var products = await _adminService.GetAllProductsAsync();
            ViewBag.Products = products
                .Where(p => p.IsAvailable)
                .OrderBy(p => p.CategoryId)
                .ThenBy(p => p.Name)
                .ToList();

            ViewBag.DaysOfWeek = DaysOfWeekDict;

            return View(menuItem);
        }

        [HttpPost]
        public async Task<IActionResult> EditWeeklyMenuItem(WeeklyMenuItemViewModel menuItem, int[] productIds, string Price, DateTime MenuDate)
        {
            var existingMenuItem = await _adminService.GetWeeklyMenuItemByIdAsync(menuItem.Id);
            if (existingMenuItem == null) return NotFound();

            menuItem.MenuDate = MenuDate.Date;
            menuItem.DayOfWeek = MenuDate.DayOfWeek;

            // Validação: não permitir menus ao fim de semana
            if (menuItem.DayOfWeek == DayOfWeek.Saturday || menuItem.DayOfWeek == DayOfWeek.Sunday)
            {
                TempData["Error"] = "Não é possível criar menus ao fim de semana!";
                
                var products = await _adminService.GetAllProductsAsync();
                ViewBag.Products = products
                    .Where(p => p.IsAvailable)
                    .OrderBy(p => p.CategoryId)
                    .ThenBy(p => p.Name)
                    .ToList();
                    
                ViewBag.DaysOfWeek = DaysOfWeekDict;

                return View(existingMenuItem);
            }

            // Verificar se já existe outro menu para a mesma data e tipo
            var allMenuItems = await _adminService.GetAllWeeklyMenuItemsAsync();
            var duplicateMenu = allMenuItems.FirstOrDefault(m =>
                m.Id != menuItem.Id &&
                m.MenuDate.Date == menuItem.MenuDate.Date &&
                m.IsVegetarian == menuItem.IsVegetarian);

            if (duplicateMenu != null)
            {
                TempData["Error"] = $"Já existe um menu {(menuItem.IsVegetarian ? "vegetariano" : "regular")} para {menuItem.MenuDate:dd/MM/yyyy}!";

                var products = await _adminService.GetAllProductsAsync();
                ViewBag.Products = products
                    .Where(p => p.IsAvailable)
                    .OrderBy(p => p.CategoryId)
                    .ThenBy(p => p.Name)
                    .ToList();

                ViewBag.DaysOfWeek = DaysOfWeekDict;

                return View(existingMenuItem);
            }

            // Configurar as propriedades do menu
            if (!string.IsNullOrWhiteSpace(Price) && TryParseDecimal(Price, out var parsedMenuPrice))
            {
                menuItem.Price = parsedMenuPrice;
            }
            
            menuItem.ProductIds = productIds?.ToList() ?? new List<int>();
            
            // Atualizar o menu via API
            var success = await _adminService.UpdateWeeklyMenuItemAsync(menuItem);
            
            if (!success)
            {
                ModelState.AddModelError("", "Erro ao atualizar menu semanal");
                var products = await _adminService.GetAllProductsAsync();
                ViewBag.Products = products
                    .Where(p => p.IsAvailable)
                    .OrderBy(p => p.CategoryId)
                    .ThenBy(p => p.Name)
                    .ToList();
                    
                ViewBag.DaysOfWeek = DaysOfWeekDict;
                return View(existingMenuItem);
            }

            TempData["Success"] = "Menu atualizado com sucesso!";
            return RedirectToAction("WeeklyMenu");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWeeklyMenuItem(int id)
        {
            // Mesmo padrão: responder em JSON quando o pedido é AJAX
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            var success = await _adminService.DeleteWeeklyMenuItemAsync(id);
            
            if (success)
            {
                if (isAjax)
                    return Json(new { success = true, message = "Menu eliminado com sucesso!" });
            }
            else if (isAjax)
            {
                return Json(new { success = false, message = "Menu não encontrado ou erro ao eliminar!" });
            }
            
            return RedirectToAction("WeeklyMenu");
        }
        private string GetDayName(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Segunda-feira",
                DayOfWeek.Tuesday => "Terça-feira",
                DayOfWeek.Wednesday => "Quarta-feira",
                DayOfWeek.Thursday => "Quinta-feira",
                DayOfWeek.Friday => "Sexta-feira",
                _ => day.ToString()
            };
        }

        private bool TryParseDecimal(string input, out decimal result)
        {
            input = input?.Trim().Replace(',', '.') ?? string.Empty;
            return decimal.TryParse(input, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out result);
        }

    }
}
