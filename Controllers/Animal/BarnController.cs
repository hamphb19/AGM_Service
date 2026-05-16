using AGM_API.Database;
using AGM_API.Models.Animal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Controllers.Animal
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BarnController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BarnController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<StallTypeInfo>>> GetStallTypes()
        {
            var types = await _context.StallTypes
                .AsNoTracking()
                .OrderBy(t => t.Id)
                .Select(t => new StallTypeInfo(t.Id, t.Name, t.ShortName))
                .ToListAsync();
            return Ok(types);
        }

        [HttpGet("animaltypes")]
        public async Task<ActionResult<IEnumerable<AnimalTypeInfo>>> GetAnimalTypes()
        {
            var types = await _context.AnimalTypes
                .AsNoTracking()
                .OrderBy(t => t.Id)
                .Select(t => new AnimalTypeInfo(t.Id, t.Name, t.ShortName))
                .ToListAsync();
            return Ok(types);
        }

        [HttpGet("farm/{farmId}")]
        public async Task<ActionResult<IEnumerable<StallInfo>>> GetStalls(long farmId)
        {
            var stalls = await _context.Stalls
                .AsNoTracking()
                .Where(s => s.FarmId == farmId)
                .Include(s => s.StallType)
                .Include(s => s.Animals).ThenInclude(a => a.AnimalType)
                .OrderBy(s => s.Name)
                .Select(s => new StallInfo(
                    s.Id,
                    s.Name,
                    new StallTypeInfo(s.StallType.Id, s.StallType.Name, s.StallType.ShortName),
                    s.Capacity,
                    s.Animals.Sum(a => a.Count),
                    s.Animals.Select(a => new StallAnimalInfo(a.Id, a.AnimalType.Name, a.AnimalType.ShortName, a.Count, a.Notes)).ToList()
                ))
                .ToListAsync();
            return Ok(stalls);
        }

        [HttpPost("farm/{farmId}")]
        public async Task<IActionResult> CreateStall(long farmId, [FromBody] UpsertStall dto)
        {
            var farm = await _context.Farms.FindAsync(farmId);
            if (farm == null) return NotFound("Farm not found");

            var stallType = await _context.StallTypes.FirstOrDefaultAsync(t => t.ShortName == dto.StallTypeShortName);
            if (stallType == null) return BadRequest("Stall type not found");

            _context.Stalls.Add(new Stall
            {
                Name = dto.Name.Trim(),
                FarmId = farmId,
                StallTypeId = stallType.Id,
                Capacity = dto.Capacity,
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStall(long id, [FromBody] UpsertStall dto)
        {
            var stall = await _context.Stalls.FindAsync(id);
            if (stall == null) return NotFound();

            var stallType = await _context.StallTypes.FirstOrDefaultAsync(t => t.ShortName == dto.StallTypeShortName);
            if (stallType == null) return BadRequest("Stall type not found");

            stall.Name = dto.Name.Trim();
            stall.StallTypeId = stallType.Id;
            stall.Capacity = dto.Capacity;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStall(long id)
        {
            var stall = await _context.Stalls.FindAsync(id);
            if (stall == null) return NotFound();
            _context.Stalls.Remove(stall);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{stallId}/animals")]
        public async Task<IActionResult> AddAnimal(long stallId, [FromBody] UpsertStallAnimal dto)
        {
            var stall = await _context.Stalls.FindAsync(stallId);
            if (stall == null) return NotFound("Stall not found");

            var animalType = await _context.AnimalTypes.FirstOrDefaultAsync(t => t.ShortName == dto.AnimalTypeShortName);
            if (animalType == null) return BadRequest("Animal type not found");

            _context.StallAnimals.Add(new StallAnimal
            {
                StallId = stallId,
                AnimalTypeId = animalType.Id,
                Count = dto.Count,
                Notes = dto.Notes,
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("animals/{id}")]
        public async Task<IActionResult> UpdateAnimal(long id, [FromBody] UpsertStallAnimal dto)
        {
            var stallAnimal = await _context.StallAnimals.FindAsync(id);
            if (stallAnimal == null) return NotFound();

            var animalType = await _context.AnimalTypes.FirstOrDefaultAsync(t => t.ShortName == dto.AnimalTypeShortName);
            if (animalType == null) return BadRequest("Animal type not found");

            stallAnimal.AnimalTypeId = animalType.Id;
            stallAnimal.Count = dto.Count;
            stallAnimal.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("animals/{id}")]
        public async Task<IActionResult> DeleteAnimal(long id)
        {
            var stallAnimal = await _context.StallAnimals.FindAsync(id);
            if (stallAnimal == null) return NotFound();
            _context.StallAnimals.Remove(stallAnimal);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{stallId}/events")]
        public async Task<ActionResult<IEnumerable<StallEventInfo>>> GetEvents(long stallId)
        {
            var events = await _context.StallEvents
                .AsNoTracking()
                .Where(e => e.StallId == stallId)
                .Include(e => e.AnimalType)
                .OrderByDescending(e => e.Date)
                .Select(e => new StallEventInfo(
                    e.Id,
                    e.Date,
                    (int)e.EventType,
                    e.AnimalType != null ? e.AnimalType.Name : null,
                    e.AnimalType != null ? e.AnimalType.ShortName : null,
                    e.Count,
                    e.Notes
                ))
                .ToListAsync();
            return Ok(events);
        }

        [HttpPost("{stallId}/events")]
        public async Task<IActionResult> CreateEvent(long stallId, [FromBody] CreateStallEvent dto)
        {
            var stall = await _context.Stalls.FindAsync(stallId);
            if (stall == null) return NotFound("Stall not found");

            AnimalType? animalType = null;
            if (!string.IsNullOrEmpty(dto.AnimalTypeShortName))
                animalType = await _context.AnimalTypes.FirstOrDefaultAsync(t => t.ShortName == dto.AnimalTypeShortName);

            _context.StallEvents.Add(new Models.Animal.StallEvent
            {
                StallId = stallId,
                Date = dto.Date,
                EventType = (Models.Animal.StallEventType)dto.EventType,
                AnimalTypeId = animalType?.Id,
                Count = dto.Count,
                Notes = dto.Notes,
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("events/{id}")]
        public async Task<IActionResult> DeleteEvent(long id)
        {
            var ev = await _context.StallEvents.FindAsync(id);
            if (ev == null) return NotFound();
            _context.StallEvents.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public record StallTypeInfo(long Id, string Name, string ShortName);
    public record AnimalTypeInfo(long Id, string Name, string ShortName);
    public record StallAnimalInfo(long Id, string AnimalTypeName, string AnimalTypeShortName, int Count, string? Notes);
    public record StallInfo(long Id, string Name, StallTypeInfo StallType, int Capacity, int TotalAnimals, List<StallAnimalInfo> Animals);
    public record UpsertStall(string Name, string StallTypeShortName, int Capacity);
    public record UpsertStallAnimal(string AnimalTypeShortName, int Count, string? Notes);
    public record CreateStallEvent(DateTime Date, int EventType, string? AnimalTypeShortName, int Count, string? Notes);
    public record StallEventInfo(long Id, DateTime Date, int EventType, string? AnimalTypeName, string? AnimalTypeShortName, int Count, string? Notes);
}
