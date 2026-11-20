using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public class FavoriteRepository
    {
        private readonly ApplicationDbContext _context;

        public FavoriteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Favorite>> GetUserFavoritesAsync(int userId)
        {
            return await _context.Favorites
                .Include(f => f.Product)
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        public async Task<Favorite> GetByIdAsync(int id)
        {
            return await _context.Favorites
                .Include(f => f.Product)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<Favorite> GetByUserAndProductAsync(int userId, int productId)
        {
            return await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
        }

        public async Task AddAsync(Favorite favorite)
        {
            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var favorite = await _context.Favorites.FindAsync(id);
            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByUserAndProductAsync(int userId, int productId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);
                
            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
        }
    }
}
