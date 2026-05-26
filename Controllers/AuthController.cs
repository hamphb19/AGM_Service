using AGM_API.Database;
using AGM_API.Models;
using AGM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AGM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public AuthController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public record RegisterRequest([Required] string Username, [Required] string Password, string? Email);
        public record LoginRequest([Required] string Username, [Required] string Password);
        public record AuthResponse(string Token, string Username, long Id, string? UserCode);
        public record ChangePasswordRequest([Required] string CurrentPassword, [Required] string NewPassword);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new { code = "USERNAME_TAKEN" });

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = _authService.HashPassword(request.Password),
                UserCode = await GenerateUniqueUserCode(),
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.Persons.Add(new Models.Person.Person { Name = request.Username, UserId = user.Id });
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
                return NotFound(new { code = "USER_NOT_FOUND" });

            if (user.PasswordHash == null || !_authService.VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized(new { code = "WRONG_PASSWORD" });

            // Backfill UserCode for accounts created before this feature
            if (user.UserCode == null)
            {
                user.UserCode = await GenerateUniqueUserCode();
                await _context.SaveChangesAsync();
            }

            var token = _authService.GenerateJwtToken(user);
            return Ok(new AuthResponse(token, user.Username, user.Id, user.UserCode));
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userId, out var id))
                return Unauthorized();

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            if (user.PasswordHash == null || !_authService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
                return BadRequest(new { code = "WRONG_PASSWORD" });

            if (request.NewPassword.Length < 6)
                return BadRequest(new { code = "PASSWORD_TOO_SHORT" });

            user.PasswordHash = _authService.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<string> GenerateUniqueUserCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rng = new Random();
            string code;
            do
            {
                code = new string(Enumerable.Repeat(chars, 6).Select(s => s[rng.Next(s.Length)]).ToArray());
            }
            while (await _context.Users.AnyAsync(u => u.UserCode == code));
            return code;
        }
    }
}
