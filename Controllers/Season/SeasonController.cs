using AGM_API.Controllers.Farm.Records;
using AGM_API.Controllers.Records;
using AGM_API.Database;
using AGM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Controllers.Season
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SeasonController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FarmAuthorizationService _auth;
        private readonly ActivityLogService _activityLog;

        public SeasonController(AppDbContext context, FarmAuthorizationService auth, ActivityLogService activityLog)
        {
            _context = context;
            _auth = auth;
            _activityLog = activityLog;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetSeasonSimple>>> GetSeasons()
        {
            var userFarmIds = await _auth.GetFarmIdsForUserAsync(User.GetUserId());

            var seasons = await _context.Seasons
                  .AsNoTracking()
                  .Include(s => s.Farm)
                  .Where(s => userFarmIds.Contains(s.Farm.Id))
                  .OrderByDescending(s => s.StartDate)
                  .Select(s => new GetSeasonSimple(
                      s.Id,
                      s.Name,
                      s.ShortName,
                      s.StartDate,
                      s.EndDate,
                      new FarmInfo(s.Farm.Id, s.Farm.Name, s.Farm.ShortName)
                  ))
                  .ToListAsync();

            return Ok(seasons);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetSeasonDetailed>> GetSeasonDetailed(long id)
        {
            var guardFarmId = await _auth.GetFarmIdForSeasonAsync(id);
            if (guardFarmId == null) return NotFound();
            if (!await _auth.IsMemberAsync(guardFarmId.Value, User.GetUserId())) return Forbid();

            var season = await _context.Seasons
                  .AsNoTracking()
                  .Include(s => s.Farm)
                  .Include(s => s.SeasonFields)
                      .ThenInclude(sf => sf.Field)
                  .Include(s => s.SeasonFields)
                      .ThenInclude(sf => sf.Crop)
                          .ThenInclude(c => c!.CropType)
                  .Include(s => s.Tasks)
                  .FirstOrDefaultAsync(s => s.Id == id);

            if (season == null)
                return NotFound();

            var result = new GetSeasonDetailed(
                season.Id,
                season.Name,
                season.ShortName,
                season.StartDate,
                season.EndDate,
                new FarmInfo(season.Farm!.Id, season.Farm!.Name, season.Farm!.ShortName),
                season.SeasonFields.Select(sf => new GetSeasonFieldInfo(
                    sf.field_Id,
                    sf.Field!.Name,
                    sf.Crop != null ? new GetCropInfo(sf.Crop.Id, sf.Crop.Name, sf.Crop.ShortName, sf.Crop.CropType!.Name) : null,
                    sf.IsPlowed,
                    sf.IsFertilized,
                    sf.IsHarvested,
                    sf.Notes
                )).ToList(),
                season.Tasks.Select(t => new GetTaskInfo(
                    t.Id,
                    t.Name,
                    t.Status.ToString(),
                    t.DueDate
                )).ToList()
            );

            return Ok(result);
        }

        [HttpGet("farm/{farmId}")]
        public async Task<ActionResult<IEnumerable<GetSeasonSimple>>> GetSeasonsByFarm(long farmId)
        {
            if (!await _auth.IsMemberAsync(farmId, User.GetUserId())) return Forbid();

            var seasons = await _context.Seasons
                .AsNoTracking()
                .Include(s => s.Farm)
                .Include(s => s.SeasonFields)
                .Include(s => s.Tasks)
                .Where(s => s.Farm.Id == farmId)
                .OrderByDescending(s => s.StartDate)
                .Select(s => new GetSeasonSimple(
                    s.Id,
                    s.Name,
                    s.ShortName,
                    s.StartDate,
                    s.EndDate,
                    new FarmInfo(s.Farm.Id, s.Farm.Name, s.Farm.ShortName)
                ))
                .ToListAsync();

            return Ok(seasons);
        }

        [HttpPost("farm/{farmId}")]
        public async Task<ActionResult> CreateSeason(long farmId, [FromBody] UpsertSeasonRequest dto)
        {
            if (!await _auth.CanWriteAsync(farmId, User.GetUserId())) return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required");

            if (!dto.StartDate.HasValue)
                return BadRequest("Start date is required");

            var farm = await _context.Farms.FindAsync(farmId);
            if (farm == null)
                return NotFound("Farm not found");

            var season = new Models.Season.Season
            {
                Name = dto.Name.Trim(),
                ShortName = dto.ShortName?.Trim() ?? string.Empty,
                StartDate = dto.StartDate.Value,
                EndDate = dto.EndDate ?? DateTime.UtcNow.AddYears(1),
                Farm = farm
            };

            _context.Seasons.Add(season);
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(farmId, "Season", season.Id, "Created",
                $"Season {season.Name} created");

            return CreatedAtAction(nameof(GetSeasonDetailed), new { id = season.Id }, new GetSeasonSimple(
                season.Id,
                season.Name,
                season.ShortName,
                season.StartDate,
                season.EndDate,
                new FarmInfo(farm.Id, farm.Name, farm.ShortName)
            ));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateSeason(long id, [FromBody] UpsertSeasonRequest dto)
        {
            var farmId = await _auth.GetFarmIdForSeasonAsync(id);
            if (farmId == null) return NotFound();
            if (!await _auth.CanWriteAsync(farmId.Value, User.GetUserId())) return Forbid();

            var season = await _context.Seasons.FindAsync(id);
            if (season == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required");

            season.Name = dto.Name.Trim();
            season.ShortName = dto.ShortName?.Trim() ?? season.ShortName;
            if (dto.StartDate.HasValue)
                season.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue)
                season.EndDate = dto.EndDate.Value;

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(farmId.Value, "Season", season.Id, "Updated",
                $"Season {season.Name} updated");

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSeason(long id)
        {
            var farmId = await _auth.GetFarmIdForSeasonAsync(id);
            if (farmId == null) return NotFound();
            if (!await _auth.IsAdminAsync(farmId.Value, User.GetUserId())) return Forbid();

            var season = await _context.Seasons
                .Include(s => s.SeasonFields)
                .Include(s => s.Tasks)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (season == null)
                return NotFound();

            // Prevent deletion if season has associated data
            if (season.SeasonFields.Any() || season.Tasks.Any())
                return BadRequest("Cannot delete season with existing fields or tasks");

            var seasonName = season.Name;
            _context.Seasons.Remove(season);
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(farmId.Value, "Season", null, "Deleted",
                $"Season {seasonName} deleted");

            return NoContent();
        }
    }
}
