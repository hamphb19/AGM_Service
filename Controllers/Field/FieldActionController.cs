using AGM_API.Database;
using AGM_API.Models.Field;
using AGM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AGM_API.Controllers.Field
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FieldActionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FarmAuthorizationService _auth;
        private readonly ActivityLogService _activity;

        public FieldActionController(AppDbContext context, FarmAuthorizationService auth, ActivityLogService activity)
        {
            _context = context;
            _auth = auth;
            _activity = activity;
        }

        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<FieldActionTypeInfo>>> GetTypes()
        {
            var types = await _context.FieldActionTypes
                .AsNoTracking()
                .OrderBy(t => t.Id)
                .Select(t => new FieldActionTypeInfo(t.Id, t.Name, t.ShortName))
                .ToListAsync();

            return Ok(types);
        }

        [HttpGet("field/{fieldId}")]
        public async Task<ActionResult<IEnumerable<FieldActionInfo>>> GetActions(long fieldId, [FromQuery] long? seasonId = null)
        {
            var farmId = await _auth.GetFarmIdForFieldAsync(fieldId);
            if (farmId == null) return NotFound("Field not found");

            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.IsMemberAsync(farmId.Value, userId))
                return Forbid();

            var query = _context.FieldActions
                .AsNoTracking()
                .Where(a => a.FieldId == fieldId);

            if (seasonId.HasValue)
                query = query.Where(a => a.SeasonId == seasonId.Value);

            var actions = await query
                .Include(a => a.ActionType)
                .Include(a => a.Machines).ThenInclude(m => m.Machine).ThenInclude(m => m.MachineType)
                .OrderByDescending(a => a.Date)
                .Select(a => new FieldActionInfo(
                    a.Id, a.FieldId, a.Date, a.ActionType.ShortName, a.ActionType.Name,
                    a.Notes, a.Amount, a.Unit,
                    a.Product, a.RegistrationNumber, a.Pest,
                    a.Temperature, a.WindSpeed, a.Applicator,
                    a.NContent, a.FertilizerType, a.Variety, a.SeasonId,
                    a.Machines.Select(m => new ActionMachineInfo(m.MachineId, m.Machine.Name, m.Machine.MachineType.ShortName)).ToList()))
                .ToListAsync();

            return Ok(actions);
        }

        [HttpGet("season/{seasonId}")]
        public async Task<ActionResult<IEnumerable<FieldActionInfo>>> GetActionsBySeason(long seasonId)
        {
            var season = await _context.Seasons
                .AsNoTracking()
                .Where(s => s.Id == seasonId)
                .Select(s => new { s.Id, FarmId = s.Farm.Id })
                .FirstOrDefaultAsync();
            if (season == null) return NotFound("Season not found");

            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.IsMemberAsync(season.FarmId, userId))
                return Forbid();

            var actions = await _context.FieldActions
                .AsNoTracking()
                .Where(a => a.SeasonId == seasonId)
                .Include(a => a.ActionType)
                .Include(a => a.Machines).ThenInclude(m => m.Machine).ThenInclude(m => m.MachineType)
                .OrderByDescending(a => a.Date)
                .Select(a => new FieldActionInfo(
                    a.Id, a.FieldId, a.Date, a.ActionType.ShortName, a.ActionType.Name,
                    a.Notes, a.Amount, a.Unit,
                    a.Product, a.RegistrationNumber, a.Pest,
                    a.Temperature, a.WindSpeed, a.Applicator,
                    a.NContent, a.FertilizerType, a.Variety, a.SeasonId,
                    a.Machines.Select(m => new ActionMachineInfo(m.MachineId, m.Machine.Name, m.Machine.MachineType.ShortName)).ToList()))
                .ToListAsync();

            return Ok(actions);
        }

        [HttpPost("field/{fieldId}")]
        public async Task<IActionResult> CreateAction(long fieldId, [FromBody] UpsertFieldAction dto)
        {
            var farmId = await _auth.GetFarmIdForFieldAsync(fieldId);
            if (farmId == null) return NotFound("Field not found");

            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.CanWriteAsync(farmId.Value, userId))
                return Forbid();

            var actionType = await _context.FieldActionTypes.FirstOrDefaultAsync(t => t.ShortName == dto.ActionTypeShortName);
            if (actionType == null) return BadRequest("Action type not found");

            var fieldName = await _context.Fields.Where(f => f.Id == fieldId).Select(f => f.Name).FirstOrDefaultAsync() ?? "";

            var action = new FieldAction
            {
                FieldId = fieldId,
                Date = dto.Date,
                ActionTypeId = actionType.Id,
                Notes = dto.Notes,
                Amount = dto.Amount,
                Unit = dto.Unit,
                Product = dto.Product,
                RegistrationNumber = dto.RegistrationNumber,
                Pest = dto.Pest,
                Temperature = dto.Temperature,
                WindSpeed = dto.WindSpeed,
                Applicator = dto.Applicator,
                NContent = dto.NContent,
                FertilizerType = dto.FertilizerType,
                Variety = dto.Variety,
                SeasonId = dto.SeasonId,
            };

            if (dto.MachineIds != null)
                foreach (var machineId in dto.MachineIds)
                    action.Machines.Add(new Models.Field.FieldActionMachine { MachineId = machineId });

            _context.FieldActions.Add(action);
            await _context.SaveChangesAsync();
            await _activity.LogAsync(farmId.Value, "FieldAction", null, "Created",
                $"{actionType.Name} · {fieldName}");
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAction(long id, [FromBody] UpsertFieldAction dto)
        {
            var action = await _context.FieldActions
                .Include(a => a.Machines)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (action == null) return NotFound();

            var farmId = await _auth.GetFarmIdForFieldAsync(action.FieldId);
            if (farmId == null) return NotFound("Field not found");

            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.CanWriteAsync(farmId.Value, userId))
                return Forbid();

            var actionType = await _context.FieldActionTypes.FirstOrDefaultAsync(t => t.ShortName == dto.ActionTypeShortName);
            if (actionType == null) return BadRequest("Action type not found");

            action.Date = dto.Date;
            action.ActionTypeId = actionType.Id;
            action.Notes = dto.Notes;
            action.Amount = dto.Amount;
            action.Unit = dto.Unit;
            action.Product = dto.Product;
            action.RegistrationNumber = dto.RegistrationNumber;
            action.Pest = dto.Pest;
            action.Temperature = dto.Temperature;
            action.WindSpeed = dto.WindSpeed;
            action.Applicator = dto.Applicator;
            action.NContent = dto.NContent;
            action.FertilizerType = dto.FertilizerType;
            action.Variety = dto.Variety;
            action.SeasonId = dto.SeasonId;

            // Sync machines
            action.Machines.Clear();
            if (dto.MachineIds != null)
                foreach (var machineId in dto.MachineIds)
                    action.Machines.Add(new Models.Field.FieldActionMachine { MachineId = machineId });

            var updActionType = await _context.FieldActionTypes.Where(t => t.Id == action.ActionTypeId).Select(t => t.Name).FirstOrDefaultAsync() ?? "";
            var updFieldName = await _context.Fields.Where(f => f.Id == action.FieldId).Select(f => f.Name).FirstOrDefaultAsync() ?? "";
            await _context.SaveChangesAsync();
            await _activity.LogAsync(farmId.Value, "FieldAction", id, "Updated",
                $"{updActionType} · {updFieldName}");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAction(long id)
        {
            var action = await _context.FieldActions.FindAsync(id);
            if (action == null) return NotFound();

            var farmId = await _auth.GetFarmIdForFieldAsync(action.FieldId);
            if (farmId == null) return NotFound("Field not found");

            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.CanWriteAsync(farmId.Value, userId))
                return Forbid();

            var delActionType = await _context.FieldActionTypes.Where(t => t.Id == action.ActionTypeId).Select(t => t.Name).FirstOrDefaultAsync() ?? "";
            var delFieldName = await _context.Fields.Where(f => f.Id == action.FieldId).Select(f => f.Name).FirstOrDefaultAsync() ?? "";
            _context.FieldActions.Remove(action);
            await _context.SaveChangesAsync();
            await _activity.LogAsync(farmId.Value, "FieldAction", id, "Deleted",
                $"{delActionType} · {delFieldName}");
            return NoContent();
        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IEnumerable<FieldActionPhotoInfo>>> GetPhotos(long id, [FromServices] IWebHostEnvironment env)
        {
            var action = await _context.FieldActions.FindAsync(id);
            if (action == null) return NotFound();

            var farmId = await _auth.GetFarmIdForFieldAsync(action.FieldId);
            if (farmId == null) return NotFound("Field not found");

            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.IsMemberAsync(farmId.Value, userId))
                return Forbid();

            var photos = await _context.FieldActionPhotos
                .AsNoTracking()
                .Where(p => p.FieldActionId == id)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();

            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            return Ok(photos.Select(p => new FieldActionPhotoInfo(
                p.Id,
                p.FileName,
                $"{baseUrl}/uploads/fieldactions/{id}/{p.FileName}"
            )));
        }

        [HttpPost("{id}/photos")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadPhoto(long id, IFormFile file, [FromServices] IWebHostEnvironment env)
        {
            var action = await _context.FieldActions.FindAsync(id);
            if (action == null) return NotFound();

            var farmId = await _auth.GetFarmIdForFieldAsync(action.FieldId);
            if (farmId == null) return NotFound("Field not found");

            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.CanWriteAsync(farmId.Value, userId))
                return Forbid();

            if (file == null || file.Length == 0)
                return BadRequest("No file provided");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
                return BadRequest("Unsupported file type");

            var folder = Path.Combine(env.WebRootPath, "uploads", "fieldactions", id.ToString());
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = System.IO.File.Create(filePath))
                await file.CopyToAsync(stream);

            _context.FieldActionPhotos.Add(new FieldActionPhoto
            {
                FieldActionId = id,
                FileName = fileName,
                CreatedAt = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync();

            var request = HttpContext.Request;
            var url = $"{request.Scheme}://{request.Host}/uploads/fieldactions/{id}/{fileName}";
            return Ok(new FieldActionPhotoInfo(0, fileName, url));
        }

        [HttpDelete("photos/{photoId}")]
        public async Task<IActionResult> DeletePhoto(long photoId, [FromServices] IWebHostEnvironment env)
        {
            var photo = await _context.FieldActionPhotos.FindAsync(photoId);
            if (photo == null) return NotFound();

            var farmId = await _auth.GetFarmIdForActionAsync(photo.FieldActionId);
            if (farmId == null) return NotFound("Action not found");

            var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (!await _auth.CanWriteAsync(farmId.Value, userId))
                return Forbid();

            var filePath = Path.Combine(env.WebRootPath, "uploads", "fieldactions",
                photo.FieldActionId.ToString(), photo.FileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.FieldActionPhotos.Remove(photo);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public record FieldActionTypeInfo(long Id, string Name, string ShortName);
    public record ActionMachineInfo(long Id, string Name, string TypeShortName);

    public record FieldActionInfo(
        long Id, long FieldId, DateTime Date,
        string ActionTypeShortName, string ActionTypeName,
        string? Notes, double? Amount, string? Unit,
        string? Product, string? RegistrationNumber, string? Pest,
        double? Temperature, double? WindSpeed, string? Applicator,
        double? NContent, int? FertilizerType, string? Variety,
        long? SeasonId,
        List<ActionMachineInfo> Machines);

    public record UpsertFieldAction(
        DateTime Date, string ActionTypeShortName,
        string? Notes, double? Amount, string? Unit,
        string? Product, string? RegistrationNumber, string? Pest,
        double? Temperature, double? WindSpeed, string? Applicator,
        double? NContent, int? FertilizerType, string? Variety,
        long? SeasonId,
        List<long>? MachineIds);

    public record FieldActionPhotoInfo(long Id, string FileName, string Url);
}
