using Microsoft.AspNetCore.Mvc;
using refeitorio.Services;

namespace refeitorio.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderService _orderService;
        private readonly AuthService _authService;

        public OrdersController(OrderService orderService, AuthService authService)
        {
            _orderService = orderService;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Obter usuário para exibir informações no cabeçalho
            var user = await _authService.GetUserById(studentId.Value);
            ViewBag.StudentName = user?.Name;
            ViewBag.Balance = user?.Balance;
            ViewBag.ProfileImage = user?.ProfileImage;

            // Obter pedidos do usuário
            var orders = await _orderService.GetUserOrdersAsync(studentId.Value);
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
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

            var order = await _orderService.GetByIdAsync(id);
            if (order == null || order.UserId != studentId.Value)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
