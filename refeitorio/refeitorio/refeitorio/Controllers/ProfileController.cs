using Microsoft.AspNetCore.Mvc;
using refeitorio.ViewModels;
using refeitorio.Services;

namespace refeitorio.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AuthService _authService;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(AuthService authService, IWebHostEnvironment environment)
        {
            _authService = authService;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _authService.GetUserById(studentId.Value);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Create ProfileViewModel with all required data
            var profileViewModel = new ProfileViewModel
            {
                Id = user.Id,
                UserNumber = user.UserNumber,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                ProfileImage = user.ProfileImage,
                UserType = user.UserType,
                Balance = user.Balance
            };
            
            return View(profileViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddBalance(string balance)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            if (studentId == null)
            {
                return isAjax 
                    ? Json(new { success = false, message = "Sessão expirada" })
                    : RedirectToAction("Login", "Auth");
            }

            // Parse decimal with comma or dot support
            if (string.IsNullOrWhiteSpace(balance))
            {
                return isAjax 
                    ? Json(new { success = false, message = "Valor inválido" })
                    : RedirectToAction("Index");
            }

            string normalizedBalance = balance.Replace(',', '.');
            if (!decimal.TryParse(normalizedBalance, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal amount))
            {
                return isAjax 
                    ? Json(new { success = false, message = "Valor inválido. Use formato: 10.50 ou 10,50" })
                    : RedirectToAction("Index");
            }

            if (amount <= 0)
            {
                return isAjax 
                    ? Json(new { success = false, message = "O valor deve ser maior que zero" })
                    : RedirectToAction("Index");
            }

            bool success = await _authService.AddBalance(studentId.Value, amount);
            if (!success)
            {
                return isAjax
                    ? Json(new { success = false, message = "Erro ao adicionar saldo" })
                    : RedirectToAction("Index");
            }

            // Obter o usuário atualizado
            var user = await _authService.GetUserById(studentId.Value);

            if (isAjax)
            {
                return Json(new
                {
                    success = true,
                    balance = user.Balance
                });
            }

            TempData["Success"] = $"Saldo adicionado com sucesso! Novo saldo: €{user.Balance:F2}";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null)
            {
                return Json(new { success = false, message = "Sessão expirada" });
            }

            if (profileImage == null || profileImage.Length == 0)
            {
                return Json(new { success = false, message = "Nenhuma imagem selecionada" });
            }

            // Validar tipo de arquivo
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Formato de imagem não suportado. Use JPG, PNG ou GIF." });
            }

            // Validar tamanho (máx 5MB)
            if (profileImage.Length > 5 * 1024 * 1024)
            {
                return Json(new { success = false, message = "A imagem deve ter no máximo 5MB." });
            }

            try
            {
                // Criar pasta profiles se não existir
                var profilesPath = Path.Combine(_environment.WebRootPath, "images", "profiles");
                if (!Directory.Exists(profilesPath))
                {
                    Directory.CreateDirectory(profilesPath);
                }

                // Gerar nome único para o arquivo
                var fileName = $"user_{studentId}{extension}";
                var filePath = Path.Combine(profilesPath, fileName);

                // Deletar imagem antiga se existir
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Salvar nova imagem
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                // Atualizar caminho no banco de dados via API
                var imageUrl = $"/images/profiles/{fileName}";
                
                // Chamar API para atualizar ProfileImage
                var updateSuccess = await _authService.UpdateProfileImage(studentId.Value, imageUrl);
                
                if (!updateSuccess)
                {
                    return Json(new { success = false, message = "Erro ao atualizar imagem no banco de dados." });
                }

                return Json(new
                {
                    success = true,
                    message = "Foto de perfil atualizada com sucesso!",
                    imageUrl = imageUrl
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao fazer upload: {ex.Message}" });
            }
        }
    }
}
