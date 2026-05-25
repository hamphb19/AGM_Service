using AGM_API.Controllers.Farm.Records;
using AGM_API.Database;
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
    public class FarmController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FarmAuthorizationService _auth;

        public FarmController(AppDbContext context, FarmAuthorizationService auth)
        {
            _context = context;
            _auth = auth;
        }

        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<FarmTypeInfo>>> GetFarmTypes()
        {
            var types = await _context.FarmTypes
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new FarmTypeInfo(t.Id, t.Name, t.ShortName))
                .ToListAsync();
            return Ok(types);
        }

        [HttpGet]
        public async Task<ActionResult<GetAllFarmsSimple>> GetFarms()
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
                .Include(m => m.Farm).ThenInclude(f => f.FarmType)
                .Select(m => new SimpleFarm(
                    m.Farm.Id,
                    m.Farm.Name,
                    m.Farm.ShortName,
                    new OwnerOfTheFarm(m.Farm.Id, m.Farm.Owner!.FirstName, m.Farm.Owner.Name),
                    m.Farm.FarmType != null ? m.Farm.FarmType.ShortName : null,
                    m.Farm.FarmType != null ? m.Farm.FarmType.Name : null,
                    m.Farm.City,
                    m.Farm.Country,
                    m.Farm.LogoUrl
                ))
                .ToListAsync();

            return Ok(new GetAllFarmsSimple(farms));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFarm(long id)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.IsMemberAsync(id, userId))
                return Forbid();

            var farm = await _context.Farms
                .AsNoTracking()
                .Include(x => x.Owner)
                .Include(x => x.FarmType)
                .SingleOrDefaultAsync(x => x.Id == id);

            if (farm == null) return NotFound();

            return Ok(new FarmDetail(
                farm.Id, farm.Name, farm.ShortName,
                new OwnerOfTheFarm(farm.Id, farm.Owner?.FirstName, farm.Owner?.Name),
                farm.FarmType?.ShortName, farm.FarmType?.Name,
                farm.Street, farm.PostalCode, farm.City, farm.Country,
                farm.Phone, farm.LogoUrl
            ));
        }

        [HttpPost]
        public async Task<IActionResult> CreateFarm([FromBody] CreateFarm dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.ShortName))
                return BadRequest("Name and ShortName are required.");

            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var personId = await _context.Persons
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => (long?)p.Id)
                .FirstOrDefaultAsync();

            if (personId == null)
            {
                var username = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.UniqueName)
                               ?? User.FindFirstValue(ClaimTypes.Name) ?? "unknown";
                var newPerson = new Models.Person.Person { Name = username, UserId = userId };
                _context.Persons.Add(newPerson);
                await _context.SaveChangesAsync();
                _context.Entry(newPerson).State = EntityState.Detached;
                personId = newPerson.Id;
            }

            var ownerRoleId = await _context.MemberRoles
                .AsNoTracking()
                .Where(r => r.ShortName == "AD")
                .Select(r => (long?)r.Id)
                .FirstOrDefaultAsync();

            if (ownerRoleId == null) return StatusCode(500, "Owner role not found.");

            long? farmTypeId = null;
            if (!string.IsNullOrWhiteSpace(dto.FarmTypeShortName))
            {
                farmTypeId = await _context.FarmTypes
                    .AsNoTracking()
                    .Where(t => t.ShortName == dto.FarmTypeShortName)
                    .Select(t => (long?)t.Id)
                    .FirstOrDefaultAsync();
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            var result = await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync();
                try
                {
                    var farm = new Models.Farm.Farm
                    {
                        Name = dto.Name.Trim(),
                        ShortName = dto.ShortName.Trim(),
                        FarmTypeId = farmTypeId,
                        Street = dto.Street?.Trim(),
                        PostalCode = dto.PostalCode?.Trim(),
                        City = dto.City?.Trim(),
                        Country = dto.Country?.Trim(),
                        Phone = dto.Phone?.Trim(),
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

                    return new SimpleFarm(farm.Id, farm.Name, farm.ShortName,
                        new OwnerOfTheFarm(farm.Id, null, null));
                }
                catch { await tx.RollbackAsync(); throw; }
            });

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFarm(long id, [FromBody] UpdateFarm dto)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.IsAdminAsync(id, userId)) return Forbid();

            var farm = await _context.Farms.FindAsync(id);
            if (farm == null) return NotFound();

            long? farmTypeId = null;
            if (!string.IsNullOrWhiteSpace(dto.FarmTypeShortName))
            {
                farmTypeId = await _context.FarmTypes
                    .AsNoTracking()
                    .Where(t => t.ShortName == dto.FarmTypeShortName)
                    .Select(t => (long?)t.Id)
                    .FirstOrDefaultAsync();
            }

            farm.Name = dto.Name.Trim();
            farm.ShortName = dto.ShortName.Trim();
            farm.FarmTypeId = farmTypeId;
            farm.Street = dto.Street?.Trim();
            farm.PostalCode = dto.PostalCode?.Trim();
            farm.City = dto.City?.Trim();
            farm.Country = dto.Country?.Trim();
            farm.Phone = dto.Phone?.Trim();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{id}/logo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadLogo(long id, IFormFile file, [FromServices] IWebHostEnvironment env)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.IsAdminAsync(id, userId)) return Forbid();

            var farm = await _context.Farms.FindAsync(id);
            if (farm == null) return NotFound();

            if (file == null || file.Length == 0) return BadRequest("No file provided.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
                return BadRequest("Unsupported file type.");

            var folder = Path.Combine(env.WebRootPath, "uploads", "farms", id.ToString());
            Directory.CreateDirectory(folder);

            var fileName = $"logo{ext}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = System.IO.File.Create(filePath))
                await file.CopyToAsync(stream);

            var request = HttpContext.Request;
            farm.LogoUrl = $"{request.Scheme}://{request.Host}/uploads/farms/{id}/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { logoUrl = farm.LogoUrl });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFarm(long id)
        {
            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.IsAdminAsync(id, userId)) return Forbid();

            var farm = await _context.Farms.FindAsync(id);
            if (farm == null) return NotFound();

            _context.Farms.Remove(farm);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public record FarmTypeInfo(long Id, string Name, string ShortName);
    public record FarmDetail(
        long Id, string Name, string ShortName,
        OwnerOfTheFarm Owner,
        string? FarmTypeShortName, string? FarmTypeName,
        string? Street, string? PostalCode, string? City, string? Country,
        string? Phone, string? LogoUrl
    );
}
