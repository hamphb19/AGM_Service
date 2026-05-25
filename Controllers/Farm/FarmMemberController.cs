using AGM_API.Database;
using AGM_API.Models.Farm.FarmMember;
using AGM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AGM_API.Controllers.Farm
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FarmMemberController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FarmAuthorizationService _auth;

        public FarmMemberController(AppDbContext context, FarmAuthorizationService auth)
        {
            _context = context;
            _auth = auth;
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleInfo>>> GetRoles()
        {
            var roles = await _context.MemberRoles
                .AsNoTracking()
                .OrderBy(r => r.Id)
                .Select(r => new RoleInfo(r.Id, r.Name, r.ShortName))
                .ToListAsync();
            return Ok(roles);
        }

        [HttpGet("farm/{farmId}")]
        public async Task<ActionResult<IEnumerable<FarmMemberInfo>>> GetMembers(long farmId)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.IsMemberAsync(farmId, userId))
                return Forbid();

            var members = await _context.FarmMembers
                .AsNoTracking()
                .Where(m => m.farm_Id == farmId)
                .Include(m => m.Person).ThenInclude(p => p.User)
                .Include(m => m.Role)
                .Select(m => new FarmMemberInfo(
                    m.person_Id,
                    m.Person.User.Username,
                    m.Person.FirstName,
                    m.Person.Name,
                    m.Role.Id,
                    m.Role.Name,
                    m.Role.ShortName
                ))
                .ToListAsync();

            return Ok(members);
        }

        [HttpGet("farm/{farmId}/findByCode")]
        public async Task<ActionResult<UserInfo>> FindByCode(long farmId, [FromQuery] string code)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.IsAdminAsync(farmId, userId))
                return Forbid();

            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.UserCode == code.Trim().ToUpperInvariant());

            if (user == null) return NotFound(new { code = "USER_NOT_FOUND" });

            return Ok(new UserInfo(user.Id, user.Username, user.Person?.FirstName, user.Person?.Name, user.UserCode));
        }

        [HttpPost("farm/{farmId}")]
        public async Task<IActionResult> AddMember(long farmId, [FromBody] AddMember dto)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.IsAdminAsync(farmId, userId))
                return Forbid();

            var farm = await _context.Farms.FindAsync(farmId);
            if (farm == null) return NotFound("Farm not found");

            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Id == dto.UserId);

            if (user == null) return BadRequest("User not found");

            var person = user.Person;
            if (person == null)
            {
                person = new Models.Person.Person { Name = user.Username, UserId = user.Id };
                _context.Persons.Add(person);
                await _context.SaveChangesAsync();
            }

            var role = await _context.MemberRoles.FirstOrDefaultAsync(r => r.ShortName == dto.RoleShortName);
            if (role == null) return BadRequest("Role not found");

            var existing = await _context.FarmMembers
                .FirstOrDefaultAsync(m => m.farm_Id == farmId && m.person_Id == person.Id && m.role_Id == role.Id);
            if (existing != null) return BadRequest("Member already added with this role");

            _context.FarmMembers.Add(new FarmMember
            {
                farm_Id = farmId,
                person_Id = person.Id,
                role_Id = role.Id,
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("farm/{farmId}/person/{personId}")]
        public async Task<IActionResult> RemoveMember(long farmId, long personId)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.IsAdminAsync(farmId, userId))
                return Forbid();

            var callerPersonId = await _context.Persons
                .Where(p => p.UserId == userId)
                .Select(p => (long?)p.Id)
                .FirstOrDefaultAsync();

            if (callerPersonId == personId)
                return BadRequest("Cannot remove yourself from the farm.");

            var members = _context.FarmMembers
                .Where(m => m.farm_Id == farmId && m.person_Id == personId);

            _context.FarmMembers.RemoveRange(members);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public record RoleInfo(long Id, string Name, string ShortName);
    public record FarmMemberInfo(long PersonId, string Username, string? FirstName, string? LastName, long RoleId, string RoleName, string RoleShortName);
    public record UserInfo(long UserId, string Username, string? FirstName, string? LastName, string? UserCode);
    public record AddMember(long UserId, string RoleShortName);
}
