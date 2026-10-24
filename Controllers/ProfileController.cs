using AGM_API.Database;
using AGM_API.Services;
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
        private readonly AccountDeletionService _accountDeletion;
        private readonly KeycloakAdminService _keycloakAdmin;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(AppDbContext context, AccountDeletionService accountDeletion, KeycloakAdminService keycloakAdmin, ILogger<ProfileController> logger)
        {
            _context = context;
            _accountDeletion = accountDeletion;
            _keycloakAdmin = keycloakAdmin;
            _logger = logger;
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

        /// <summary>
        /// Löscht das eigene Konto endgültig (Apple Guideline 5.1.1(v)):
        /// zuerst das Login-Konto in Keycloak, dann die lokalen Daten (siehe AccountDeletionService).
        /// </summary>
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMe()
        {
            var userId = User.GetUserId();
            var subject = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.KeycloakSubject)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(subject))
            {
                var result = await _keycloakAdmin.DeleteUserAsync(subject);
                if (result == KeycloakDeleteResult.Failed)
                    return StatusCode(StatusCodes.Status502BadGateway, "Could not delete the login account. Please try again later.");
                if (result == KeycloakDeleteResult.NotConfigured)
                    _logger.LogWarning("Deleting user {UserId} locally only — Keycloak admin client is not configured.", userId);
            }

            if (!await _accountDeletion.DeleteAsync(userId)) return NotFound();
            return NoContent();
        }
    }
}
