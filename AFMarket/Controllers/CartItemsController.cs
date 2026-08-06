using AFMarket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AFMarket.Controllers;

public class CartItemsController : Controller
{
    private readonly RequestDbContext _context;

    public CartItemsController(RequestDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Cart()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null)
        {
            return RedirectToAction("Login", "Users");
        }

        var cartItems = await _context.CartItems
            .Include(item => item.Product)
            .Where(item => item.UserId == userId)
            .ToListAsync();

        ViewBag.Orders = await _context.Orders
            .Include(order => order.OrderItems)
            .ThenInclude(item => item.Product)
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.OrderDate)
            .ToListAsync();

        return View(cartItems);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, string action)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null)
        {
            return RedirectToAction("Login", "Users");
        }

        var cartItem = await _context.CartItems
            .Include(item => item.Product)
            .FirstOrDefaultAsync(item => item.CartItemId == cartItemId && item.UserId == userId);

        if (cartItem is null)
        {
            return NotFound();
        }

        if (action == "increase" && cartItem.Quantity < cartItem.Product.Stock)
        {
            cartItem.Quantity++;
        }
        else if (action == "decrease" && cartItem.Quantity > 1)
        {
            cartItem.Quantity--;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null)
        {
            return RedirectToAction("Login", "Users");
        }

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(item => item.CartItemId == cartItemId && item.UserId == userId);

        if (cartItem is not null)
        {
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null)
        {
            return RedirectToAction("Login", "Users");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            return RedirectToAction("Login", "Users");
        }

        var cartItems = await _context.CartItems
            .Include(item => item.Product)
            .Where(item => item.UserId == userId)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return RedirectToAction(nameof(Cart));
        }

        if (cartItems.Any(item => item.Quantity <= 0 || item.Quantity > item.Product.Stock))
        {
            TempData["ErrorMessage"] = "One or more products do not have enough stock.";
            return RedirectToAction(nameof(Cart));
        }

        var totalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity);
        if (user.Balance < totalAmount)
        {
            TempData["ErrorMessage"] = "Insufficient demo balance.";
            return RedirectToAction(nameof(Cart));
        }

        var order = new Order
        {
            UserId = userId.Value,
            OrderDate = DateTime.UtcNow,
            Status = "Completed",
            TotalAmount = totalAmount
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var item in cartItems)
        {
            _context.OrderItems.Add(new OrderItem
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Product.Price
            });
            item.Product.Stock -= item.Quantity;
        }

        user.Balance -= totalAmount;
        _context.CartItems.RemoveRange(cartItems);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        TempData["SuccessMessage"] = "Purchase completed successfully.";
        return RedirectToAction(nameof(Cart));
    }
}
