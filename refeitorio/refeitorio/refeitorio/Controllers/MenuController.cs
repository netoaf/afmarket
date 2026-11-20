using Microsoft.AspNetCore.Mvc;
using refeitorio.Services;
using refeitorio.ViewModels;

namespace refeitorio.Controllers
{
    public class MenuController : Controller
    {
        private readonly MenuTicketService _menuTicketService;
        private readonly AuthService _authService;
        private readonly ILogger<MenuController> _logger;
        
        public MenuController(MenuTicketService menuTicketService, AuthService authService, ILogger<MenuController> logger)
        {
            _menuTicketService = menuTicketService;
            _authService = authService;
            _logger = logger;
        }
        public async Task<IActionResult> Index(bool vegetarian = false, DateTime? weekStart = null)
        {
            _logger.LogInformation("MenuController.Index called");
            
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                _logger.LogWarning("No StudentId in session, redirecting to login");
                return RedirectToAction("Login", "Auth");
            }

            var today = DateTime.Today;
            var currentWeekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var selectedWeekStart = weekStart ?? currentWeekStart;

            _logger.LogInformation($"Today: {today:yyyy-MM-dd} ({today.DayOfWeek}), Week start: {selectedWeekStart:yyyy-MM-dd}");

            var user = await _authService.GetUserById(studentId.Value);
            ViewBag.StudentName = user?.Name;
            ViewBag.Balance = user?.Balance;
            ViewBag.ProfileImage = user?.ProfileImage;

            var viewModel = new WeeklyMenuViewModel 
            { 
                ShowVegetarian = vegetarian,
                CurrentWeekStart = currentWeekStart,
                WeekStart = selectedWeekStart
            };

            // Obter menus da API
            _logger.LogInformation("Fetching regular menu...");
            viewModel.RegularMenu = await _menuTicketService.GetMenuForWeekAsync(selectedWeekStart, false);
            _logger.LogInformation("Fetching vegetarian menu...");
            viewModel.VegetarianMenu = await _menuTicketService.GetMenuForWeekAsync(selectedWeekStart, true);
            
            _logger.LogInformation($"Regular menu items: {viewModel.RegularMenu?.Count ?? 0}");
            _logger.LogInformation($"Vegetarian menu items: {viewModel.VegetarianMenu?.Count ?? 0}");
            
            // Obter todos os produtos da API para exibir nas descrições do menu
            var allProducts = await _menuTicketService.GetAllProductsAsync();

            var daysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
            var now = DateTime.Now;

            // Obter tickets do usuário
            var userTickets = await _menuTicketService.GetUserTicketsAsync(studentId.Value);

            // Preparar dicionário de produtos para cada dia da semana
            var menuProducts = new Dictionary<DayOfWeek, List<ProductViewModel>>();
            
            foreach (var day in daysOfWeek)
            {
                var targetDate = selectedWeekStart.AddDays((int)day - (int)DayOfWeek.Monday);
                
                // Obter o menu para este dia, dependendo se é vegetariano ou não
                var menu = vegetarian ? viewModel.VegetarianMenu : viewModel.RegularMenu;
                if (menu.ContainsKey(day))
                {
                    var menuItem = menu[day];
                    // Obter produtos deste menu (simplified for now)
                    menuProducts[day] = allProducts
                        .Where(p => p.IsAvailable)
                        .ToList();
                }
                
                // Verificar tickets existentes para este dia
                var anyTicket = userTickets.FirstOrDefault(t => t.Menu != null && t.Menu.MenuDate.Date == targetDate.Date);
                var currentTypeTicket = userTickets.FirstOrDefault(t => 
                    t.Menu != null && t.Menu.MenuDate.Date == targetDate.Date && 
                    t.Menu.IsVegetarian == vegetarian);

                viewModel.HasTicket[day] = currentTypeTicket != null;

                // Verificar se pode comprar tickets (sem ticket existente e antes das 10h do dia)
                var canBuy = anyTicket == null && (targetDate.Date > today || (targetDate.Date == today && now.Hour < 10));
                ViewBag.CanBuy = ViewBag.CanBuy ?? new Dictionary<DayOfWeek, bool>();
                ((Dictionary<DayOfWeek, bool>)ViewBag.CanBuy)[day] = canBuy;

                // Passa info sobre se tem ticket do outro tipo
                ViewBag.HasOtherTypeTicket = ViewBag.HasOtherTypeTicket ?? new Dictionary<DayOfWeek, bool>();
                ((Dictionary<DayOfWeek, bool>)ViewBag.HasOtherTypeTicket)[day] = anyTicket != null && currentTypeTicket == null;
                
                // Tipo do ticket que o utilizador tem (se tiver)
                ViewBag.ExistingTicketType = ViewBag.ExistingTicketType ?? new Dictionary<DayOfWeek, string>();
                if (anyTicket != null)
                {
                    ((Dictionary<DayOfWeek, string>)ViewBag.ExistingTicketType)[day] = anyTicket.Menu.IsVegetarian ? "vegetariano" : "regular";
                }

                // Só pode cancelar se: tem ticket E (dia futuro OU dia de hoje antes das 9:30)
                var canCancel = currentTypeTicket != null && 
                    (targetDate.Date > today || (targetDate.Date == today && (now.Hour < 9 || (now.Hour == 9 && now.Minute <= 30))));
                ViewBag.CanCancel = ViewBag.CanCancel ?? new Dictionary<DayOfWeek, bool>();
                ((Dictionary<DayOfWeek, bool>)ViewBag.CanCancel)[day] = canCancel;
            }

            // Adicionar dicionário de produtos ao ViewBag
            ViewBag.MenuProducts = menuProducts;

            return View(viewModel);
        }

        public async Task<IActionResult> History()
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

            var today = DateTime.Today;

            // Obter tickets do usuário via API
            var tickets = await _menuTicketService.GetUserTicketsAsync(studentId.Value);
            
            // Ordenar os tickets
            tickets = tickets
                .Where(t => t.Menu != null)
                .OrderByDescending(t => t.Menu.MenuDate)
                .ThenByDescending(t => t.CreatedAt)
                .ToList();

            return View(tickets);
        }

        [HttpPost]
        public async Task<IActionResult> BuyTicket(DayOfWeek dayOfWeek, bool isVegetarian, DateTime? weekStart = null)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            // Detetamos pedidos AJAX (Fetch) para devolver JSON em vez de redirecionar
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            if (studentId == null)
                return isAjax ? Json(new { success = false, message = "Sessão expirada" }) : RedirectToAction("Login", "Auth");

            var now = DateTime.Now;
            var today = DateTime.Today;
            var currentWeekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var selectedWeekStart = weekStart ?? currentWeekStart;
            var targetDate = selectedWeekStart.AddDays((int)dayOfWeek - (int)DayOfWeek.Monday);

            // Verificar horário limite para compra de ticket
            if (targetDate.Date == today && now.Hour >= 10)
            {
                var msg = "A compra de almoços só é permitida até às 10:00 do próprio dia!";
                if (isAjax) return Json(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToAction("Index", new { vegetarian = isVegetarian });
            }

            // Obter tickets do usuário via API
            var userTickets = await _menuTicketService.GetUserTicketsAsync(studentId.Value);
            
            // Verifica se já tem qualquer ticket para este dia (vegetariano OU regular)
            var existingTicket = userTickets.FirstOrDefault(t => t.Menu != null && t.Menu.MenuDate.Date == targetDate.Date);

            if (existingTicket != null)
            {
                var tipoExistente = existingTicket.Menu.IsVegetarian ? "vegetariano" : "regular";
                var msg = $"Já possui um ticket {tipoExistente} para este dia!";
                if (isAjax) return Json(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToAction("Index", new { vegetarian = isVegetarian });
            }

            // Obter o menu para o dia selecionado
            var menus = await _menuTicketService.GetWeeklyMenuAsync(targetDate, targetDate.AddDays(1), isVegetarian);
            var menuItem = menus.FirstOrDefault(m => m.MenuDate.Date == targetDate.Date && m.IsVegetarian == isVegetarian);

            if (menuItem == null)
            {
                var msg = "Menu não disponível para este dia!";
                if (isAjax) return Json(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToAction("Index", new { vegetarian = isVegetarian });
            }

            // Verificar limite de tickets para este menu
            if (menuItem.CurrentTickets >= menuItem.MaxTickets)
            {
                var msg = "Este menu está esgotado!";
                if (isAjax) return Json(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToAction("Index", new { vegetarian = isVegetarian });
            }

            // Verificar saldo do usuário
            var user = await _authService.GetUserById(studentId.Value);
            if (user == null)
            {
                var msg = "Usuário não encontrado!";
                if (isAjax) return Json(new { success = false, message = msg });
                return RedirectToAction("Index", new { vegetarian = isVegetarian });
            }

            if (user.Balance < menuItem.Price)
            {
                var msg = "Saldo insuficiente!";
                if (isAjax) return Json(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToAction("Index", new { vegetarian = isVegetarian });
            }

            // Comprar o ticket via API (passar a data correta)
            var ticket = await _menuTicketService.BuyTicketAsync(studentId.Value, menuItem.Id, targetDate);

            if (ticket == null)
            {
                var msg = "Erro ao comprar ticket. Tente novamente.";
                if (isAjax) return Json(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToAction("Index", new { vegetarian = isVegetarian });
            }

            // Obter o usuário atualizado para verificar o novo saldo
            user = await _authService.GetUserById(studentId.Value);

            var successMsg = $"Ticket comprado com sucesso! Novo saldo: €{user.Balance:F2}";
            
            if (isAjax)
                // Fluxo assíncrono devolve payload para atualizar UI (mensagem, saldo, reload)
                return Json(new { 
                    success = true,
                    message = successMsg,
                    balance = user.Balance,
                    reload = true
                });
            
            TempData["Success"] = successMsg;
            return RedirectToAction("Index", new { vegetarian = isVegetarian });
        }

        [HttpPost]
        public async Task<IActionResult> CancelTicket(DayOfWeek dayOfWeek, bool isVegetarian, DateTime? weekStart = null)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            // Tal como em BuyTicket, suportamos cancelamento via AJAX
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            if (studentId == null)
                return isAjax ? Json(new { success = false, message = "Sessão expirada" }) : RedirectToAction("Login", "Auth");

            var now = DateTime.Now;
            var today = DateTime.Today;
            var currentWeekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            var selectedWeekStart = weekStart ?? currentWeekStart;
            var targetDate = selectedWeekStart.AddDays((int)dayOfWeek - (int)DayOfWeek.Monday);

            // Verificar horário limite para cancelamento
            if (targetDate.Date == today && (now.Hour > 9 || (now.Hour == 9 && now.Minute > 30)))
            {
                var msg = "O cancelamento só é permitido até às 09:30 do próprio dia!";
                if (isAjax) return Json(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToAction("Index", new { vegetarian = isVegetarian });
            }

            // Obter tickets do usuário
            var userTickets = await _menuTicketService.GetUserTicketsAsync(studentId.Value);
            var ticket = userTickets.FirstOrDefault(t =>
                t.Menu != null &&
                t.IsVegetarian == isVegetarian &&
                t.Menu.MenuDate.Date == targetDate.Date);

            if (ticket == null)
            {
                var msg = "Ticket não encontrado!";
                if (isAjax) return Json(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToAction("Index", new { vegetarian = isVegetarian });
            }

            // Guardar o preço do ticket para exibir na mensagem
            decimal ticketPrice = ticket.Price;

            // Cancelar o ticket via API
            bool success = await _menuTicketService.CancelTicketAsync(ticket.Id);

            if (!success)
            {
                var msg = "Erro ao cancelar ticket. Tente novamente.";
                if (isAjax) return Json(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToAction("Index", new { vegetarian = isVegetarian });
            }

            // Obter saldo atualizado
            var user = await _authService.GetUserById(studentId.Value);
            if (user == null)
            {
                var msg = "Usuário não encontrado!";
                if (isAjax) return Json(new { success = false, message = msg });
                return RedirectToAction("Index", new { vegetarian = isVegetarian });
            }

            var successMsg = $"Ticket cancelado com sucesso! Valor devolvido: €{ticketPrice:F2}. Novo saldo: €{user.Balance:F2}";
            
            if (isAjax)
                // Resposta em JSON para permitir feedback instantâneo no cliente
                return Json(new { 
                    success = true,
                    message = successMsg,
                    balance = user.Balance,
                    reload = true
                });
            
            TempData["Success"] = successMsg;
            return RedirectToAction("Index", new { vegetarian = isVegetarian });
        }
    }
}
