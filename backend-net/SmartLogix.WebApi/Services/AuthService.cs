using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SmartLogix.WebApi.Data;
using SmartLogix.WebApi.DTOs;
using SmartLogix.WebApi.Models;

namespace SmartLogix.WebApi.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto dto);
        Task<AuthResponseDto?> RegisterAsync(RegisterDto dto);
    }

    public class AuthService : IAuthService
    {
        private readonly SmartLogixDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(SmartLogixDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public Task<AuthResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == dto.Username);
            if (user == null || !DbInitializer.VerifyPassword(dto.Password, user.PasswordHash))
                return Task.FromResult<AuthResponseDto?>(null);

            return Task.FromResult<AuthResponseDto?>(BuildAuthResponse(user));
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto dto)
        {
            if (_context.Users.Any(u => u.Username == dto.Username))
                return null; // Username already taken

            var newUser = new User
            {
                Username = dto.Username,
                PasswordHash = DbInitializer.HashPassword(dto.Password),
                Role = dto.Role == "Admin" ? "Admin" : "User",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return BuildAuthResponse(newUser);
        }

        private AuthResponseDto BuildAuthResponse(User user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "SmartLogix_Default_JWT_SecretKey_2026_Must_Be_32_Chars!";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "SmartLogixGateway";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "SmartLogixClients";
            var expireHours = int.Parse(_configuration["Jwt:ExpireHours"] ?? "8");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddHours(expireHours);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("uid", user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Username = user.Username,
                Role = user.Role,
                ExpiresAt = expiresAt
            };
        }
    }
}
