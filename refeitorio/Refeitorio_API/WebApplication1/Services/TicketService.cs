using WebApplication1.DTOs;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services
{
    public class TicketService
    {
        private readonly TicketRepository _ticketRepository;
        private readonly WeeklyMenuRepository _menuRepository;
        private readonly UserRepository _userRepository;

        public TicketService(
            TicketRepository ticketRepository,
            WeeklyMenuRepository menuRepository,
            UserRepository userRepository)
        {
            _ticketRepository = ticketRepository;
            _menuRepository = menuRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<TicketDTO>> GetAllAsync(int? userId = null, DateTime? date = null)
        {
            var tickets = await _ticketRepository.GetAllAsync(userId, date);
            return tickets.Select(MapToDTO);
        }

        public async Task<TicketDTO> GetByIdAsync(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
                return null;

            return MapToDTO(ticket);
        }

        public async Task<TicketDTO> CreateAsync(CreateTicketDTO ticketDto)
        {
            var user = await _userRepository.GetByIdAsync(ticketDto.UserId);
            if (user == null)
                return null;

            var menu = await _menuRepository.GetByIdAsync(ticketDto.MenuId);
            if (menu == null)
                return null;

            if (!await _ticketRepository.CanMakeReservationAsync(ticketDto.MenuId))
                return null;

            if (!await _ticketRepository.IsBeforeCutoffTimeAsync(ticketDto.MenuId))
                return null;

            var existingTickets = await _ticketRepository.GetAllAsync(ticketDto.UserId, null);
            var hasTicketForSameDay = existingTickets.Any(t => 
                t.Menu != null && 
                t.Menu.MenuDate.Date == menu.MenuDate.Date);
            
            if (hasTicketForSameDay)
                return null;

            if (user.Balance < menu.Price)
                return null;

            user.Balance -= menu.Price;
            await _userRepository.UpdateAsync(user);

            var ticket = new Ticket
            {
                UserId = ticketDto.UserId,
                MenuId = ticketDto.MenuId,
                ReservationDate = DateTime.Now,
                IsUsed = false
            };

            await _ticketRepository.AddAsync(ticket);

            var createdTicket = await _ticketRepository.GetByIdAsync(ticket.Id);
            if (createdTicket == null)
                return null;

            return MapToDTO(createdTicket);
        }

        public async Task<bool> CancelAsync(int id)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
                return false;

            // Verificar se ainda é possível cancelar (antes das 9:30 do dia)
            if (!await _ticketRepository.IsBeforeCancellationTimeAsync(ticket.MenuId))
                return false;

            // Verificar se o ticket já foi usado
            if (ticket.IsUsed)
                return false;

            // Retornar o valor para o saldo do usuário
            var user = await _userRepository.GetByIdAsync(ticket.UserId);
            if (user == null)
                return false;

            var menu = await _menuRepository.GetByIdAsync(ticket.MenuId);
            if (menu == null)
                return false;

            user.Balance += menu.Price;
            await _userRepository.UpdateAsync(user);

            // Excluir o ticket
            await _ticketRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> ValidateAsync(int id, int staffId)
        {
            var ticket = await _ticketRepository.GetByIdAsync(id);
            if (ticket == null)
                return false;

            // Verificar se o funcionário existe e tem permissão
            var staff = await _userRepository.GetByIdAsync(staffId);
            if (staff == null || staff.UserType != "Staff" && staff.UserType != "Admin")
                return false;

            // Verificar se o ticket já foi usado
            if (ticket.IsUsed)
                return false;

            // Validar o ticket
            ticket.IsUsed = true;
            ticket.UsedAt = DateTime.Now;
            ticket.ValidatedByStaffId = staffId;

            await _ticketRepository.UpdateAsync(ticket);
            return true;
        }

        private TicketDTO MapToDTO(Ticket ticket)
        {
            return new TicketDTO
            {
                Id = ticket.Id,
                UserId = ticket.UserId,
                UserName = ticket.User?.Name,
                MenuId = ticket.MenuId,
                MenuDate = ticket.Menu?.MenuDate ?? DateTime.MinValue,
                Price = ticket.Menu?.Price ?? 0m,
                IsVegetarian = ticket.Menu?.IsVegetarian ?? false,
                ReservationDate = ticket.ReservationDate,
                IsUsed = ticket.IsUsed,
                UsedAt = ticket.UsedAt,
                ValidatedByStaffId = ticket.ValidatedByStaffId
            };
        }
    }
}
