using Microsoft.AspNetCore.Mvc;
using SmartLogix.WebApi.DTOs;
using SmartLogix.WebApi.Services;

namespace SmartLogix.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Authenticates a user and returns a signed JWT access token.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Username and password are required." });

            var result = await _authService.LoginAsync(dto);
            if (result == null)
                return Unauthorized(new { message = "Invalid username or password." });

            return Ok(result);
        }

        /// <summary>
        /// Registers a new user and returns a signed JWT access token.
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Username and password are required." });

            if (dto.Password.Length < 6)
                return BadRequest(new { message = "Password must be at least 6 characters long." });

            var result = await _authService.RegisterAsync(dto);
            if (result == null)
                return Conflict(new { message = $"Username '{dto.Username}' is already taken." });

            return CreatedAtAction(nameof(Login), result);
        }
    }
}
