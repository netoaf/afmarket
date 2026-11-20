using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllUsers()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserDTO userDto)
        {
            if (id != userDto.Id)
                return BadRequest();

            var result = await _userService.UpdateAsync(userDto);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPost("{id}/addbalance")]
        public async Task<IActionResult> AddBalance(int id, [FromBody] decimal amount)
        {
            var result = await _userService.AddBalanceAsync(id, amount);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpPut("{id}/profileimage")]
        public async Task<IActionResult> UpdateProfileImage(int id, [FromBody] string imageUrl)
        {
            var result = await _userService.UpdateProfileImageAsync(id, imageUrl);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
