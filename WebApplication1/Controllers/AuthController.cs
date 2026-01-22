using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly JwtService _jwtService;

        public AuthController(ApplicationDbContext context, PasswordHasher passwordHasher, JwtService jwtService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        //Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Пользователь с таким именем уже существует");
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest("Пользователь с таким email уже существует");
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            user.Token = _jwtService.GenerateToken(user);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = user.Token,
                CreatedAt = user.CreatedAt
            };

            return Ok(response);
        }

        //Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized("Неверное имя пользователя или пароль");
            }

            user.Token = _jwtService.GenerateToken(user);
            user.LastLogin = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = user.Token,
                LastLogin = user.LastLogin
            };

            return Ok(response);
        }

        //Auth/change-password
        [HttpPatch("change-password")]
        public async Task<ActionResult<AuthResponse>> ChangePassword(ChangePasswordRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Unauthorized();
            }

            if (!_passwordHasher.VerifyPassword(request.OldPassword, user.PasswordHash))
            {
                return BadRequest("Неверный старый пароль");
            }

            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.Token = _jwtService.GenerateToken(user); // Новый токен

            await _context.SaveChangesAsync();

            var response = new AuthResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = user.Token,
                Message = "Пароль успешно изменен"
            };

            return Ok(response);
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public string? Message { get; set; }
    }
}