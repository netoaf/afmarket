using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AFMarket.Models;

namespace AFMarket.Controllers
{
    public class ProductsController : Controller
    {
        private readonly RequestDbContext _context;

        public ProductsController(RequestDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string searchString, int? categoryId, bool? favoritos)
        {
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.CurrentFilter = searchString;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.MostrarFavoritos = favoritos;

            var products = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            if (categoryId != null)
            {
                products = products.Where(p => p.CategoryId == categoryId);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var favIds = await _context.Users
                    .Include(u => u.Products)
                    .Where(u => u.UserId == userId)
                    .SelectMany(u => u.Products.Select(p => p.ProductId))
                    .ToListAsync();

                ViewBag.Favorites = favIds;

                if (favoritos == true)
                {
                    products = products.Where(p => favIds.Contains(p.ProductId));
                }
            }

            return View(await products.OrderBy(p => p.Name).ToListAsync());
        }



        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET
        public IActionResult Create()
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, string? NewCategory)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            if (!string.IsNullOrWhiteSpace(NewCategory))
            {
                var category = new Category { Name = NewCategory };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                product.CategoryId = category.CategoryId;
            }

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryList = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,Stock,Price,CategoryId,Details")] Product product)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin())
            {
                return Forbid();
            }

            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null || quantity <= 0 || quantity > product.Stock)
            {
                TempData["ErrorMessage"] = "Stock indisponível";
                return RedirectToAction("Index");
            }

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (cartItem != null)
            {
                if (cartItem.Quantity + quantity > product.Stock)
                {
                    TempData["ErrorMessage"] = "A quantidade pedida excede o stock disponível";
                    return RedirectToAction("Index");
                }

                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    UserId = userId.Value,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult ToggleFavorite(int productId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            var user = _context.Users.Include(u => u.Products).FirstOrDefault(u => u.UserId == userId);
            if (user == null)
                return RedirectToAction("Login", "Users");

            var product = _context.Products.Find(productId);
            if (product == null)
                return NotFound();

            if (user.Products.Contains(product))
                user.Products.Remove(product);
            else
                user.Products.Add(product);

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        private bool IsAdmin() =>
            string.Equals(
                HttpContext.Session.GetString("UserRole"),
                "Admin",
                StringComparison.OrdinalIgnoreCase);

    }
}
