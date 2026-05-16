using AGM_API.Controllers.Farm.Records;
using AGM_API.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AGM_API.Controllers.Farm
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FarmController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FarmController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Farm.Farm>>> GetFarms()
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var person = await _context.Persons
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (person == null)
                return Ok(new GetAllFarmsSimple(new List<SimpleFarm>()));

            var farms = await _context.FarmMembers
                .AsNoTracking()
                .Where(m => m.person_Id == person.Id)
                .Include(m => m.Farm).ThenInclude(f => f.Owner)
                .Select(m => new SimpleFarm(
                    m.Farm.Id,
                    m.Farm.Name,
                    m.Farm.ShortName,
                    new OwnerOfTheFarm(m.Farm.Id, m.Farm.Owner.FirstName, m.Farm.Owner.Name)
                ))
                .ToListAsync();

            return Ok(new GetAllFarmsSimple(farms));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Farm.Farm>> GetFarm(long id)
        {
            var farm = await _context.Farms
                .AsNoTracking()
                .Include(x => x.Owner)
                .SingleOrDefaultAsync(x => x.Id == id);

            if (farm == null)
                return NotFound();

            return Ok(new SimpleFarm(farm.Id, farm.Name, farm.ShortName, new OwnerOfTheFarm(farm.Id, farm.Owner?.FirstName, farm.Owner?.Name)));
        }

        [HttpPost]
        public async Task<ActionResult<Models.Farm.Farm>> CreateAFarm([FromBody] CreateSimpleFarm farmDto)
        {
            if (string.IsNullOrWhiteSpace(farmDto.Name) || string.IsNullOrWhiteSpace(farmDto.ShortName))
                return BadRequest("Name and ShortName are required Fields!");

            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var personId = await _context.Persons
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => (long?)p.Id)
                .FirstOrDefaultAsync();

            if (personId == null)
            {
                var username = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName)
                               ?? User.FindFirstValue(ClaimTypes.Name)
                               ?? "unknown";
                var newPerson = new Models.Person.Person { Name = username, UserId = userId };
                _context.Persons.Add(newPerson);
                await _context.SaveChangesAsync();
                _context.Entry(newPerson).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                personId = newPerson.Id;
            }

            var ownerRoleId = await _context.MemberRoles
                .AsNoTracking()
                .Where(r => r.ShortName == "AD")
                .Select(r => (long?)r.Id)
                .FirstOrDefaultAsync();

            if (ownerRoleId == null)
                return StatusCode(500, "Owner role not found.");

            var executionStrategy = _context.Database.CreateExecutionStrategy();

            var result = await executionStrategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    var farm = new Models.Farm.Farm
                    {
                        Name = farmDto.Name.Trim(),
                        ShortName = farmDto.ShortName.Trim(),
                    };

                    _context.Farms.Add(farm);
                    await _context.SaveChangesAsync();

                    _context.FarmMembers.Add(new Models.Farm.FarmMember.FarmMember
                    {
                        farm_Id = farm.Id,
                        person_Id = personId.Value,
                        role_Id = ownerRoleId.Value,
                    });
                    await _context.SaveChangesAsync();

                    await tx.CommitAsync();

                    return new SimpleFarm(farm.Id, farm.Name, farm.ShortName, new OwnerOfTheFarm(farm.Id, null, null));
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFarm(long id)
        {
            var farm = await _context.Farms.FindAsync(id);
            if (farm == null)
            {
                return NotFound();
            }

            _context.Farms.Remove(farm);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        #region HelperMethods

        private bool FarmExists(long id)
        {
            return _context.Farms.Any(e => e.Id == id);
        }


        #endregion

    }
}
