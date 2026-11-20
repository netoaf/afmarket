using WebApplication1.Models;
using WebApplication1.Repositories;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Services
{
    public class CategoryDeleteResult
    {
        public bool Success { get; set; }
        public bool HasAssociatedProducts { get; set; }
        public int DeletedProductsCount { get; set; }
    }

    public class CategoryService
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;

        public CategoryService(CategoryRepository categoryRepository, ApplicationDbContext context)
        {
            _categoryRepository = categoryRepository;
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<Category> CreateAsync(string name)
        {
            var category = new Category { Name = name };
            await _categoryRepository.AddAsync(category);
            return category;
        }

        public async Task<bool> UpdateAsync(int id, string name)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return false;

            category.Name = name;
            await _categoryRepository.UpdateAsync(category);
            return true;
        }

        public async Task<CategoryDeleteResult> DeleteAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    Console.WriteLine($"Category with ID {id} not found");
                    return new CategoryDeleteResult { Success = false };
                }

                Console.WriteLine($"Attempting to delete category: {category.Name} (ID: {id})");

                // Use a transaction to ensure atomicity
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Get all products in this category
                    var productsInCategory = await _context.Products
                        .Where(p => p.CategoryId == id)
                        .ToListAsync();

                    var productIds = productsInCategory.Select(p => p.Id).ToList();

                    var result = new CategoryDeleteResult 
                    { 
                        Success = true, 
                        HasAssociatedProducts = productsInCategory.Any(),
                        DeletedProductsCount = productsInCategory.Count
                    };

                    if (productsInCategory.Any())
                    {
                        Console.WriteLine($"Found {productsInCategory.Count} products, checking for dependencies...");

                        // Check and delete from WeeklyMenu (MainDish, Soup, Dessert)
                        var weeklyMenuItems = await _context.WeeklyMenu
                            .Where(wm => productIds.Contains(wm.MainDishId) || 
                                       (wm.SoupId.HasValue && productIds.Contains(wm.SoupId.Value)) || 
                                       (wm.DessertId.HasValue && productIds.Contains(wm.DessertId.Value)))
                            .ToListAsync();
                        
                        if (weeklyMenuItems.Any())
                        {
                            Console.WriteLine($"Deleting {weeklyMenuItems.Count} weekly menu items...");
                            _context.WeeklyMenu.RemoveRange(weeklyMenuItems);
                            await _context.SaveChangesAsync();
                        }

                        // Check and delete from Tickets
                        var tickets = await _context.Tickets
                            .Where(t => productIds.Contains(t.MenuId))
                            .ToListAsync();
                        
                        if (tickets.Any())
                        {
                            Console.WriteLine($"Deleting {tickets.Count} tickets...");
                            _context.Tickets.RemoveRange(tickets);
                            await _context.SaveChangesAsync();
                        }

                        // Check and delete from Cart
                        var cartItems = await _context.Cart
                            .Where(c => productIds.Contains(c.ProductId))
                            .ToListAsync();
                        
                        if (cartItems.Any())
                        {
                            Console.WriteLine($"Deleting {cartItems.Count} cart items...");
                            _context.Cart.RemoveRange(cartItems);
                            await _context.SaveChangesAsync();
                        }

                        // Check and delete from OrderItems
                        var orderItems = await _context.OrderItems
                            .Where(oi => productIds.Contains(oi.ProductId))
                            .ToListAsync();
                        
                        if (orderItems.Any())
                        {
                            Console.WriteLine($"Deleting {orderItems.Count} order items...");
                            _context.OrderItems.RemoveRange(orderItems);
                            await _context.SaveChangesAsync();
                        }

                        // Check and delete from Favorites
                        var favorites = await _context.Favorites
                            .Where(f => productIds.Contains(f.ProductId))
                            .ToListAsync();
                        
                        if (favorites.Any())
                        {
                            Console.WriteLine($"Deleting {favorites.Count} favorites...");
                            _context.Favorites.RemoveRange(favorites);
                            await _context.SaveChangesAsync();
                        }

                        // Finally delete the products
                        Console.WriteLine($"Deleting {productsInCategory.Count} products...");
                        _context.Products.RemoveRange(productsInCategory);
                        await _context.SaveChangesAsync();
                        
                        Console.WriteLine($"WARNING: Deleted {productsInCategory.Count} products and all their dependencies for category '{category.Name}'");
                    }

                    // Delete the category directly
                    _context.Categories.Remove(category);
                    await _context.SaveChangesAsync();
                    
                    // Commit the transaction
                    await transaction.CommitAsync();
                    Console.WriteLine($"Successfully deleted category '{category.Name}'");
                    return result;
                }
                catch (Exception ex)
                {
                    // Rollback the transaction if anything fails
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Transaction rolled back due to error: {ex.Message}");
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting category {id}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new CategoryDeleteResult { Success = false };
            }
        }
    }
}
