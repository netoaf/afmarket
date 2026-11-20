using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly TicketService _ticketService;

        public TicketsController(TicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDTO>>> GetAllTickets([FromQuery] int? userId = null, [FromQuery] DateTime? date = null)
        {
            var tickets = await _ticketService.GetAllAsync(userId, date);
            return Ok(tickets);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDTO>> GetTicket(int id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);
            if (ticket == null)
                return NotFound();

            return Ok(ticket);
        }

        [HttpPost]
        public async Task<ActionResult<TicketDTO>> CreateTicket(CreateTicketDTO ticketDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ticket = await _ticketService.CreateAsync(ticketDto);
            if (ticket == null)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Não foi possível criar o ticket. Verifique: saldo insuficiente, vagas esgotadas, horário de marcação expirado, ou já possui ticket para este dia." 
                });
            }

            return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelTicket(int id)
        {
            var result = await _ticketService.CancelAsync(id);
            if (!result)
                return BadRequest("Não foi possível cancelar o ticket. Verifique o horário de cancelamento.");

            return NoContent();
        }

        [HttpPut("{id}/validate")]
        public async Task<IActionResult> ValidateTicket(int id, [FromBody] int staffId)
        {
            var result = await _ticketService.ValidateAsync(id, staffId);
            if (!result)
                return BadRequest("Não foi possível validar o ticket.");

            return NoContent();
        }
    }
}
