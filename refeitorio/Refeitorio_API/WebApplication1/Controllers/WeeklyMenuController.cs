using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeeklyMenuController : ControllerBase
    {
        private readonly WeeklyMenuService _menuService;

        public WeeklyMenuController(WeeklyMenuService menuService)
        {
            _menuService = menuService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WeeklyMenuDTO>>> GetWeeklyMenu([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null, [FromQuery] bool? isVegetarian = null)
        {
            var menu = await _menuService.GetMenuAsync(startDate, endDate, isVegetarian);
            return Ok(menu);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WeeklyMenuDTO>> GetMenuItem(int id)
        {
            var menuItem = await _menuService.GetByIdAsync(id);
            if (menuItem == null)
                return NotFound();

            return Ok(menuItem);
        }

        [HttpPost]
        public async Task<ActionResult<WeeklyMenuDTO>> CreateMenuItem(CreateWeeklyMenuDTO menuDto)
        {
            var menuItem = await _menuService.CreateAsync(menuDto);
            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuItem(int id, CreateWeeklyMenuDTO menuDto)
        {
            var result = await _menuService.UpdateAsync(id, menuDto);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var result = await _menuService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
