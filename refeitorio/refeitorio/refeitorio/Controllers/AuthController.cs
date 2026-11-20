using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using refeitorio.Services;

namespace refeitorio.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("StudentId") != null)
            {
                return RedirectToAction("Index", "Products");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string userNumber, string password)
        {
            // Verifica se os dados de login foram fornecidos
            if (string.IsNullOrEmpty(userNumber) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Número de usuário e senha são obrigatórios";
                return View();
            }

            // Verifica se o pedido veio via AJAX (Fetch) para responder em JSON em vez de redirecionar
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            try
            {
                Console.WriteLine($"Login attempt: UserNumber={userNumber}");
                
                var user = await _authService.Login(userNumber, password);

                if (user != null)
                {
                    try 
                    {
                        HttpContext.Session.SetInt32("StudentId", user.Id);
                        HttpContext.Session.SetString("StudentName", user.Name ?? "Usuário");
                        HttpContext.Session.SetString("UserType", user.UserType ?? "Student");
                        
                        Console.WriteLine($"Login successful: UserType={user.UserType}");
                        
                        if (isAjax)
                            // Em AJAX devolvemos JSON com info e URL para redirecionamento no cliente
                            return Json(new { success = true, message = "Login realizado com sucesso!", redirect = Url.Action("Index", "Products") });
                        
                        return RedirectToAction("Index", "Products");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao processar login: {ex.Message}");
                        if (isAjax)
                            return Json(new { success = false, message = "Erro ao processar login. Tente novamente." });
                            
                        ViewBag.Error = "Erro ao processar login. Tente novamente.";
                        return View();
                    }
                }

                if (isAjax)
                    // Em caso de erro via AJAX respondemos em JSON sem recarregar pagina
                    return Json(new { success = false, message = "Credenciais inválidas" });
                
                ViewBag.Error = "Credenciais inválidas";
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no processo de login: {ex.Message}");
                
                if (isAjax)
                    return Json(new { success = false, message = "Ocorreu um erro durante o login. Tente novamente." });
                    
                ViewBag.Error = "Ocorreu um erro durante o login. Tente novamente.";
                return View();
            }
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string userNumber, string password, string name, string? email = null, string? phone = null)
        {
            try
            {
                // Verifica se os dados obrigatórios foram fornecidos
                if (string.IsNullOrEmpty(userNumber) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(name))
                {
                    ViewBag.Error = "Número de usuário, senha e nome são obrigatórios";
                    return View();
                }
                
                // Mesma verificação de AJAX para adaptar resposta
                bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                
                // Tentar registrar diretamente via API
                var user = await _authService.Register(userNumber, password, name, email, phone);
                
                if (user == null)
                {
                    if (isAjax)
                        // Se pedido for assíncrono devolvemos mensagem sem recarregar
                        return Json(new { success = false, message = "Este número de usuário já existe. Escolhe outro." });
                    
                    ViewBag.Error = "Este número de usuário já existe. Escolhe outro.";
                    return View();
                }

                try {
                    HttpContext.Session.SetInt32("StudentId", user.Id);
                    HttpContext.Session.SetString("StudentName", user.Name ?? "Usuário");
                    HttpContext.Session.SetString("UserType", user.UserType ?? "Student");
                    
                    if (isAjax)
                        // Sucesso em AJAX devolve JSON com mensagem e rota de destino
                        return Json(new { success = true, message = "Conta criada com sucesso!", redirect = Url.Action("Index", "Products") });
                    
                    return RedirectToAction("Index", "Products");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar registro: {ex.Message}");
                    if (isAjax)
                        return Json(new { success = false, message = "Erro ao processar registro. Tente novamente." });
                        
                    ViewBag.Error = "Erro ao processar registro. Tente novamente.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no processo de registro: {ex.Message}");
                
                bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
                if (isAjax)
                    return Json(new { success = false, message = "Ocorreu um erro durante o registro. Tente novamente." });
                    
                ViewBag.Error = "Ocorreu um erro durante o registro. Tente novamente.";
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
