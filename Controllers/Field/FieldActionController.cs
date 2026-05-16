using AGM_API.Database;
using AGM_API.Models.Field;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Controllers.Field
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FieldActionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FieldActionController(AppDbContext context)
        {
            _context = context;
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
        public async Task<ActionResult<IEnumerable<FieldActionInfo>>> GetActions(long fieldId)
        {
            var actions = await _context.FieldActions
                .AsNoTracking()
                .Where(a => a.FieldId == fieldId)
                .Include(a => a.ActionType)
                .OrderByDescending(a => a.Date)
                .Select(a => new FieldActionInfo(a.Id, a.FieldId, a.Date, a.ActionType.ShortName, a.ActionType.Name, a.Notes, a.Amount, a.Unit))
                .ToListAsync();

            return Ok(actions);
        }

        [HttpPost("field/{fieldId}")]
        public async Task<IActionResult> CreateAction(long fieldId, [FromBody] UpsertFieldAction dto)
        {
            var field = await _context.Fields.FindAsync(fieldId);
            if (field == null) return NotFound("Field not found");

            var actionType = await _context.FieldActionTypes.FirstOrDefaultAsync(t => t.ShortName == dto.ActionTypeShortName);
            if (actionType == null) return BadRequest("Action type not found");

            _context.FieldActions.Add(new FieldAction
            {
                FieldId = fieldId,
                Date = dto.Date,
                ActionTypeId = actionType.Id,
                Notes = dto.Notes,
                Amount = dto.Amount,
                Unit = dto.Unit,
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAction(long id, [FromBody] UpsertFieldAction dto)
        {
            var action = await _context.FieldActions.FindAsync(id);
            if (action == null) return NotFound();

            var actionType = await _context.FieldActionTypes.FirstOrDefaultAsync(t => t.ShortName == dto.ActionTypeShortName);
            if (actionType == null) return BadRequest("Action type not found");

            action.Date = dto.Date;
            action.ActionTypeId = actionType.Id;
            action.Notes = dto.Notes;
            action.Amount = dto.Amount;
            action.Unit = dto.Unit;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAction(long id)
        {
            var action = await _context.FieldActions.FindAsync(id);
            if (action == null) return NotFound();

            _context.FieldActions.Remove(action);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/photos")]
        public async Task<ActionResult<IEnumerable<FieldActionPhotoInfo>>> GetPhotos(long id, [FromServices] IWebHostEnvironment env)
        {
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
    public record FieldActionInfo(long Id, long FieldId, DateTime Date, string ActionTypeShortName, string ActionTypeName, string? Notes, double? Amount, string? Unit);
    public record UpsertFieldAction(DateTime Date, string ActionTypeShortName, string? Notes, double? Amount, string? Unit);
    public record FieldActionPhotoInfo(long Id, string FileName, string Url);
}
