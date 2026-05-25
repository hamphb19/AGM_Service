using AGM_API.Controllers.Farm.Records;
using AGM_API.Controllers.Records;
using AGM_API.Database;
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

        public SeasonController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetSeasonSimple>>> GetSeasons()
        {
            var seasons = await _context.Seasons
                  .AsNoTracking()
                  .Include(s => s.Farm)
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

        [HttpPost]
        public async Task<ActionResult> CreateSeason([FromBody] CreateSeason dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Name is required");

            var farm = await _context.Farms.FindAsync(dto.FarmId);
            if (farm == null)
                return BadRequest("Farm not found");

            var season = new Models.Season.Season
            {
                Name = dto.Name.Trim(),
                ShortName = dto.ShortName.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Farm = farm
            };

            _context.Seasons.Add(season);
            await _context.SaveChangesAsync();

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
        public async Task<ActionResult> UpdateSeason(long id, [FromBody] UpdateSeason dto)
        {
            var season = await _context.Seasons.FindAsync(id);
            if (season == null)
                return NotFound();

            season.Name = dto.Name.Trim();
            season.ShortName = dto.ShortName.Trim();
            season.StartDate = dto.StartDate;
            season.EndDate = dto.EndDate;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteSeason(long id)
        {
            var season = await _context.Seasons.FindAsync(id);
            if (season == null)
                return NotFound();

            _context.Seasons.Remove(season);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
