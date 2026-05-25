using AGM_API.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AGM_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProfileController(AppDbContext context)
        {
            _context = context;
        }

        public record ProfileInfo(long Id, string Username, string? Email, string? UserCode, string? FirstName, string LastName);
        public record UpdateProfile(string? FirstName, string LastName, string? Email);

        [HttpGet("me")]
        public async Task<ActionResult<ProfileInfo>> GetMe()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            return Ok(new ProfileInfo(
                user.Id,
                user.Username,
                user.Email,
                user.UserCode,
                user.Person?.FirstName,
                user.Person?.Name ?? ""
            ));
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfile dto)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            user.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            if (user.Person != null)
            {
                user.Person.FirstName = string.IsNullOrWhiteSpace(dto.FirstName) ? null : dto.FirstName.Trim();
                user.Person.Name = string.IsNullOrWhiteSpace(dto.LastName) ? user.Person.Name : dto.LastName.Trim();
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
