using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public class ProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(ApplicationDbContext context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(int? categoryId = null, bool? isVegetarian = null)
        {
            // DEBUG: Log da connection string
            _logger.LogInformation($"[DEBUG] Connection String: {_context.Database.GetConnectionString()}");
            _logger.LogInformation($"[DEBUG] Database Name: {_context.Database.GetDbConnection().Database}");
            
            // Forçar limpeza completa do cache do EF
            _context.ChangeTracker.Clear();
            
            // Usar SQL direto para contornar cache do EF
            var products = await _context.Products
                .FromSqlRaw("SELECT * FROM Products")
                .AsNoTracking()
                .ToListAsync();
            
            // Aplicar filtros em memória
            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId.Value).ToList();

            if (isVegetarian.HasValue)
                products = products.Where(p => p.IsVegetarian == isVegetarian.Value).ToList();
            
            // DEBUG: Log ImageUrl lido da BD
            foreach (var p in products.Take(3))
            {
                _logger.LogInformation($"[API-BD] Product: {p.Name}, ImageUrl: {p.ImageUrl}");
            }
            
            return products;
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateStockAsync(int id, int quantity)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.Stock = quantity;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Product>> SearchAsync(string query, decimal? minPrice = null, decimal? maxPrice = null, string allergens = null)
        {
            query = string.IsNullOrWhiteSpace(query) ? null : query.Trim();
            allergens = string.IsNullOrWhiteSpace(allergens) ? null : allergens.Trim();
            
            IQueryable<Product> productsQuery = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsAvailable);

            if (query != null)
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(query));
            }

            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
            }

            if (allergens != null)
            {
                productsQuery = productsQuery.Where(p => p.Allergens == null || !p.Allergens.Contains(allergens));
            }

            return await productsQuery.ToListAsync();
        }
    }
}
