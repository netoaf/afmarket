using AFMarket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFMarket.Controllers;

public class UsersController : Controller
{
    private readonly RequestDbContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UsersController(RequestDbContext context, IPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<IActionResult> Index()
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        return View(await _context.Users.AsNoTracking().ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        if (id is null)
        {
            return NotFound();
        }

        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.UserId == id);

        return user is null ? NotFound() : View(user);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var currentUserId = HttpContext.Session.GetInt32("UserId");
        if (id is null || currentUserId is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (id != currentUserId)
        {
            return Forbid();
        }

        var user = await _context.Users.FindAsync(id);
        return user is null ? NotFound() : View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, string username, string email, string? newPassword)
    {
        var currentUserId = HttpContext.Session.GetInt32("UserId");
        if (currentUserId is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (id != currentUserId)
        {
            return Forbid();
        }

        var user = await _context.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        username = username.Trim();
        email = email.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(string.Empty, "Username and email are required.");
        }

        if (await _context.Users.AnyAsync(candidate =>
                candidate.UserId != id &&
                (candidate.Username == username || candidate.Email == email)))
        {
            ModelState.AddModelError(string.Empty, "Username or email is already in use.");
        }

        if (!string.IsNullOrEmpty(newPassword) && newPassword.Length < 8)
        {
            ModelState.AddModelError(string.Empty, "The new password must have at least 8 characters.");
        }

        if (!ModelState.IsValid)
        {
            user.Username = username;
            user.Email = email;
            return View(user);
        }

        user.Username = username;
        user.Email = email;

        if (!string.IsNullOrEmpty(newPassword))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
        }

        await _context.SaveChangesAsync();
        TempData["Message"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        if (id is null)
        {
            return NotFound();
        }

        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.UserId == id);

        return user is null ? NotFound() : View(user);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var user = await _context.Users.FindAsync(id);
        if (user is not null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Login() => View();

    public IActionResult Register() => View();

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    public async Task<IActionResult> Profile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.UserId == userId);

        return user is null ? NotFound() : View(user);
    }

    [HttpPost]
    public async Task<IActionResult> AddBalance(decimal amount)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null)
        {
            return RedirectToAction(nameof(Login));
        }

        if (amount <= 0 || amount > 1000)
        {
            TempData["ErrorMessage"] = "Enter an amount between 0.01 and 1000.";
            return RedirectToAction(nameof(Profile));
        }

        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            return RedirectToAction(nameof(Login));
        }

        user.Balance += amount;
        _context.Transactions.Add(new Transaction
        {
            UserId = user.UserId,
            Amount = amount,
            TransactionDate = DateTime.UtcNow,
            Description = "Demo balance top-up"
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    public async Task<IActionResult> Authenticate(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(candidate => candidate.Username == username);

        if (user is not null &&
            _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) !=
            PasswordVerificationResult.Failed)
        {
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetInt32("UserId", user.UserId);
            return RedirectToAction("Index", "Products");
        }

        ViewBag.ErrorMessage = "Invalid username or password.";
        return View(nameof(Login));
    }

    [HttpPost]
    public async Task<IActionResult> NewUser(
        string username,
        string email,
        string password,
        string cpassword)
    {
        username = username.Trim();
        email = email.Trim();

        if (password != cpassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
        }

        if (password.Length < 8)
        {
            ModelState.AddModelError(string.Empty, "Password must have at least 8 characters.");
        }

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(string.Empty, "Username and email are required.");
        }

        if (await _context.Users.AnyAsync(candidate =>
                candidate.Username == username || candidate.Email == email))
        {
            ModelState.AddModelError(string.Empty, "Username or email is already in use.");
        }

        if (!ModelState.IsValid)
        {
            return View(nameof(Register));
        }

        var user = new User
        {
            Role = "User",
            Username = username,
            Email = email,
            Balance = 100,
            RegistrationDate = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Login));
    }

    private bool IsAdmin() =>
        string.Equals(
            HttpContext.Session.GetString("UserRole"),
            "Admin",
            StringComparison.OrdinalIgnoreCase);
}
