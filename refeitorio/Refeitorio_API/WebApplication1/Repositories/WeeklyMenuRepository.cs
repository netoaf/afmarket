using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public class WeeklyMenuRepository
    {
        private readonly ApplicationDbContext _context;

        public WeeklyMenuRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WeeklyMenu>> GetAllAsync(DateTime? startDate = null, DateTime? endDate = null, bool? isVegetarian = null)
        {
            IQueryable<WeeklyMenu> query = _context.WeeklyMenu
                .Include(wm => wm.MainDish)
                .Include(wm => wm.Soup)
                .Include(wm => wm.Dessert);

            if (startDate.HasValue)
                query = query.Where(wm => wm.MenuDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(wm => wm.MenuDate <= endDate.Value);

            if (isVegetarian.HasValue)
                query = query.Where(wm => wm.IsVegetarian == isVegetarian.Value);

            return await query.ToListAsync();
        }

        public async Task<WeeklyMenu> GetByIdAsync(int id)
        {
            return await _context.WeeklyMenu
                .Include(wm => wm.MainDish)
                .Include(wm => wm.Soup)
                .Include(wm => wm.Dessert)
                .FirstOrDefaultAsync(wm => wm.Id == id);
        }

        public async Task<int> GetCurrentReservationCountAsync(int menuId)
        {
            return await _context.Tickets
                .CountAsync(t => t.MenuId == menuId);
        }

        public async Task AddAsync(WeeklyMenu weeklyMenu)
        {
            _context.WeeklyMenu.Add(weeklyMenu);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(WeeklyMenu weeklyMenu)
        {
            _context.WeeklyMenu.Update(weeklyMenu);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var weeklyMenu = await _context.WeeklyMenu.FindAsync(id);
            if (weeklyMenu != null)
            {
                _context.WeeklyMenu.Remove(weeklyMenu);
                await _context.SaveChangesAsync();
            }
        }
    }
}
