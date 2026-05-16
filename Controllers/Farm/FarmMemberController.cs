using AGM_API.Database;
using AGM_API.Models.Farm.FarmMember;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Controllers.Farm
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FarmMemberController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FarmMemberController(AppDbContext context)
        {
            _context = context;
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

        [HttpGet("farm/{farmId}/available")]
        public async Task<ActionResult<IEnumerable<UserInfo>>> GetAvailableUsers(long farmId)
        {
            var memberPersonIds = await _context.FarmMembers
                .Where(m => m.farm_Id == farmId)
                .Select(m => m.person_Id)
                .Distinct()
                .ToListAsync();

            var available = await _context.Users
                .AsNoTracking()
                .Include(u => u.Person)
                .Where(u => u.Person != null && !memberPersonIds.Contains(u.Person.Id))
                .Select(u => new UserInfo(u.Id, u.Username, u.Person!.FirstName, u.Person!.Name))
                .ToListAsync();

            return Ok(available);
        }

        [HttpPost("farm/{farmId}")]
        public async Task<IActionResult> AddMember(long farmId, [FromBody] AddMember dto)
        {
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
            var members = _context.FarmMembers
                .Where(m => m.farm_Id == farmId && m.person_Id == personId);

            _context.FarmMembers.RemoveRange(members);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public record RoleInfo(long Id, string Name, string ShortName);
    public record FarmMemberInfo(long PersonId, string Username, string? FirstName, string? LastName, long RoleId, string RoleName, string RoleShortName);
    public record UserInfo(long UserId, string Username, string? FirstName, string? LastName);
    public record AddMember(long UserId, string RoleShortName);
}
