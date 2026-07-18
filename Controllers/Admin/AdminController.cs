using AGM_API.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Controllers.Admin
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // ── Stats ────────────────────────────────────────────────────────────

        [HttpGet("stats")]
        public async Task<ActionResult<AdminStats>> GetStats()
        {
            var userCount = await _context.Users.CountAsync();
            var farmCount = await _context.Farms.CountAsync();
            var fieldCount = await _context.Fields.CountAsync();
            var actionCount = await _context.FieldActions.CountAsync();

            return Ok(new AdminStats(userCount, farmCount, fieldCount, actionCount));
        }

        // ── Users ─────────────────────────────────────────────────────────────

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<AdminUserInfo>>> GetUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Username)
                .Select(u => new AdminUserInfo(u.Id, u.Username, u.Email, u.UserCode, u.IsAdmin))
                .ToListAsync();

            return Ok(users);
        }

        [HttpPatch("users/{userId}/admin")]
        public async Task<IActionResult> SetAdmin(long userId, [FromBody] SetAdminRequest dto)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.IsAdmin = dto.IsAdmin;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ── Farms ─────────────────────────────────────────────────────────────

        [HttpGet("farms")]
        public async Task<ActionResult<IEnumerable<AdminFarmInfo>>> GetFarms()
        {
            var farms = await _context.Farms
                .AsNoTracking()
                .Include(f => f.FarmType)
                .Include(f => f.farmMembers)
                .OrderBy(f => f.Name)
                .Select(f => new AdminFarmInfo(f.Id, f.Name, f.ShortName, f.FarmType != null ? f.FarmType.Name : null, f.farmMembers.Count))
                .ToListAsync();

            return Ok(farms);
        }

        // ── Crops ─────────────────────────────────────────────────────────────

        [HttpGet("crops")]
        public async Task<ActionResult<IEnumerable<AdminCropInfo>>> GetCrops()
        {
            var crops = await _context.Crops
                .AsNoTracking()
                .Include(c => c.CropType)
                .OrderBy(c => c.CropType.Name).ThenBy(c => c.Name)
                .Select(c => new AdminCropInfo(c.Id, c.Name, c.ShortName, c.CropType.Id, c.CropType.Name))
                .ToListAsync();

            return Ok(crops);
        }

        [HttpGet("croptypes")]
        public async Task<ActionResult<IEnumerable<AdminCropTypeInfo>>> GetCropTypes()
        {
            var types = await _context.CropTypes
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .Select(t => new AdminCropTypeInfo(t.Id, t.Name, t.ShortName))
                .ToListAsync();

            return Ok(types);
        }

        [HttpPost("crops")]
        public async Task<IActionResult> CreateCrop([FromBody] UpsertCropRequest dto)
        {
            var cropType = await _context.CropTypes.FindAsync(dto.CropTypeId);
            if (cropType == null) return BadRequest("CropType not found");

            _context.Crops.Add(new Models.Crop.Crop { Name = dto.Name, ShortName = dto.ShortName, CropTypeId = dto.CropTypeId });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("crops/{id}")]
        public async Task<IActionResult> UpdateCrop(long id, [FromBody] UpsertCropRequest dto)
        {
            var crop = await _context.Crops.FindAsync(id);
            if (crop == null) return NotFound();

            crop.Name = dto.Name;
            crop.ShortName = dto.ShortName;
            crop.CropTypeId = dto.CropTypeId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("crops/{id}")]
        public async Task<IActionResult> DeleteCrop(long id)
        {
            var crop = await _context.Crops.FindAsync(id);
            if (crop == null) return NotFound();

            _context.Crops.Remove(crop);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("croptypes")]
        public async Task<IActionResult> CreateCropType([FromBody] UpsertCropTypeRequest dto)
        {
            _context.CropTypes.Add(new Models.Crop.CropType { Name = dto.Name, ShortName = dto.ShortName });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("croptypes/{id}")]
        public async Task<IActionResult> UpdateCropType(long id, [FromBody] UpsertCropTypeRequest dto)
        {
            var type = await _context.CropTypes.FindAsync(id);
            if (type == null) return NotFound();

            type.Name = dto.Name;
            type.ShortName = dto.ShortName;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("croptypes/{id}")]
        public async Task<IActionResult> DeleteCropType(long id)
        {
            var type = await _context.CropTypes.FindAsync(id);
            if (type == null) return NotFound();

            _context.CropTypes.Remove(type);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public record AdminStats(int Users, int Farms, int Fields, int FieldActions);
    public record AdminUserInfo(long Id, string Username, string? Email, string? UserCode, bool IsAdmin);
    public record AdminFarmInfo(long Id, string Name, string ShortName, string? FarmType, int MemberCount);
    public record AdminCropInfo(long Id, string Name, string ShortName, long CropTypeId, string CropTypeName);
    public record AdminCropTypeInfo(long Id, string Name, string ShortName);
    public record UpsertCropRequest(string Name, string ShortName, long CropTypeId);
    public record UpsertCropTypeRequest(string Name, string ShortName);
    public record SetAdminRequest(bool IsAdmin);
}
