using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;

        public AuthController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDto)
        {
            var user = await _userService.AuthenticateAsync(loginDto);
            
            if (user == null)
                return Unauthorized("Credenciais inválidas");

            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(RegisterUserDTO registerDto)
        {
            var existingUser = await _userService.GetByUserNumberAsync(registerDto.UserNumber);
            if (existingUser != null)
                return BadRequest("Usuário já existe");

            // Public registration can only create students; never trust a client-supplied role.
            registerDto.UserType = "Student";
            var user = await _userService.CreateAsync(registerDto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }
    }
}
