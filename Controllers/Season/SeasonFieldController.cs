using AGM_API.Controllers.Records;
using AGM_API.Database;
using AGM_API.Models.Season;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Controllers.Season
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SeasonFieldController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SeasonFieldController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetSeasonFieldInfo>>> GetSeasonFields(long seasonId)
        {
            var season = await _context.Seasons.FindAsync(seasonId);
            if (season == null)
                return NotFound("Season not found");

            var seasonFields = await _context.SeasonFields
                .AsNoTracking()
                .Where(sf => sf.season_Id == seasonId)
                .Include(sf => sf.Field)
                .Include(sf => sf.Crop)
                    .ThenInclude(c => c.CropType)
                .Select(sf => new GetSeasonFieldInfo(
                    sf.field_Id,
                    sf.Field!.Name,
                    sf.Crop != null ? new GetCropInfo(sf.Crop.Id, sf.Crop.Name, sf.Crop.ShortName, sf.Crop.CropType!.Name) : null,
                    sf.IsPlowed,
                    sf.IsFertilized,
                    sf.IsHarvested,
                    sf.Notes
                ))
                .ToListAsync();

            return Ok(seasonFields);
        }

        [HttpGet("{fieldId}")]
        public async Task<ActionResult<GetSeasonFieldInfo>> GetSeasonField(long seasonId, long fieldId)
        {
            var seasonField = await _context.SeasonFields
                .AsNoTracking()
                .Include(sf => sf.Field)
                .Include(sf => sf.Crop)
                    .ThenInclude(c => c!.CropType)
                .FirstOrDefaultAsync(sf => sf.season_Id == seasonId && sf.field_Id == fieldId);

            if (seasonField == null)
                return NotFound();

            return Ok(new GetSeasonFieldInfo(
                seasonField.field_Id,
                seasonField.Field!.Name,
                seasonField.Crop != null ? new GetCropInfo(seasonField.Crop.Id, seasonField.Crop.Name, seasonField.Crop.ShortName, seasonField.Crop.CropType!.Name) : null,
                seasonField.IsPlowed,
                seasonField.IsFertilized,
                seasonField.IsHarvested,
                seasonField.Notes
            ));
        }

        [HttpPut("{fieldId}")]
        public async Task<ActionResult> UpdateSeasonField(long seasonId, long fieldId, [FromBody] UpdateSeasonField dto)
        {
            var seasonField = await _context.SeasonFields
                .FirstOrDefaultAsync(sf => sf.season_Id == seasonId && sf.field_Id == fieldId);

            if (seasonField == null)
                return NotFound();

            seasonField.Crop = dto.CropId.HasValue
                ? await _context.Crops.FindAsync(dto.CropId.Value)
                : null;
            seasonField.IsPlowed = dto.IsPlowed;
            seasonField.IsFertilized = dto.IsFertilized;
            seasonField.IsHarvested = dto.IsHarvested;
            seasonField.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult> AddFieldToSeason(long seasonId, [FromBody] AddFieldToSeason dto)
        {
            var season = await _context.Seasons.FindAsync(seasonId);
            if (season == null)
                return NotFound("Season not found");

            var field = await _context.Fields.FindAsync(dto.FieldId);
            if (field == null)
                return BadRequest("Field not found");

            var existing = await _context.SeasonFields
                .FirstOrDefaultAsync(sf => sf.season_Id == seasonId && sf.field_Id == dto.FieldId);

            var crop = await _context.Crops
                .FirstOrDefaultAsync(x => x.Id == dto.CropId);

            if (existing != null)
                return BadRequest("Field already added to this season");

            var seasonField = new SeasonField
            {
                season_Id = seasonId,
                field_Id = dto.FieldId,
                Crop = crop,
                Notes = dto.Notes
            };

            _context.SeasonFields.Add(seasonField);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
