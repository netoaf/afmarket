using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public class TicketRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync(int? userId = null, DateTime? date = null)
        {
            IQueryable<Ticket> query = _context.Tickets
                .Include(t => t.User)
                .Include(t => t.Menu)
                .Include(t => t.ValidatedByStaff);

            if (userId.HasValue)
                query = query.Where(t => t.UserId == userId.Value);

            if (date.HasValue)
                query = query.Where(t => t.Menu.MenuDate.Date == date.Value.Date);

            return await query.ToListAsync();
        }

        public async Task<Ticket> GetByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.User)
                .Include(t => t.Menu)
                .Include(t => t.ValidatedByStaff)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> CanMakeReservationAsync(int menuId)
        {
            // Verificar se já atingiu o limite de vagas
            var menu = await _context.WeeklyMenu.FindAsync(menuId);
            if (menu == null)
                return false;

            var currentCount = await _context.Tickets.CountAsync(t => t.MenuId == menuId);
            return currentCount < menu.MaxReservations;
        }

        public async Task<bool> IsBeforeCutoffTimeAsync(int menuId)
        {
            var menu = await _context.WeeklyMenu.FindAsync(menuId);
            if (menu == null)
                return false;

            // Verificar se está antes das 10:00 do dia do menu
            var cutoffTime = menu.MenuDate.Date.AddHours(10);
            return DateTime.Now < cutoffTime;
        }

        public async Task<bool> IsBeforeCancellationTimeAsync(int menuId)
        {
            var menu = await _context.WeeklyMenu.FindAsync(menuId);
            if (menu == null)
                return false;

            // Verificar se está antes das 9:30 do dia do menu
            var cutoffTime = menu.MenuDate.Date.AddHours(9).AddMinutes(30);
            return DateTime.Now < cutoffTime;
        }

        public async Task AddAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
        }
    }
}
