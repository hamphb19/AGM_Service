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
    public class ActivityController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FarmAuthorizationService _auth;

        public ActivityController(AppDbContext context, FarmAuthorizationService auth)
        {
            _context = context;
            _auth = auth;
        }

        [HttpGet("farm/{farmId}")]
        public async Task<IActionResult> GetFarmActivity(long farmId, [FromQuery] int limit = 50)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.IsMemberAsync(farmId, userId))
                return Forbid();

            var entries = await _context.ActivityLogs
                .AsNoTracking()
                .Where(a => a.FarmId == farmId)
                .OrderByDescending(a => a.Timestamp)
                .Take(Math.Min(limit, 100))
                .Select(a => new ActivityEntry(
                    a.Id, a.EntityType, a.EntityId, a.Action,
                    a.Description, a.UserDisplayName, a.Timestamp))
                .ToListAsync();

            return Ok(entries);
        }
    }

    public record ActivityEntry(
        long Id, string EntityType, long? EntityId, string Action,
        string Description, string UserDisplayName, DateTime Timestamp);
}
