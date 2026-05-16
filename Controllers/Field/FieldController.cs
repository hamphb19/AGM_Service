using AGM_API.Controllers.Records;
using AGM_API.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace AGM_API.Controllers.Field
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FieldController : ControllerBase
    {
        private readonly AppDbContext _context;
        private static readonly GeometryFactory _geoFactory =
            NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        public FieldController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("farm/{farmId}")]
        public async Task<ActionResult<IEnumerable<GetFieldDetail>>> GetFields(long farmId)
        {
            var fields = await _context.Fields
                .AsNoTracking()
                .Where(f => f.FarmId == farmId)
                .Include(f => f.keyPoints)
                .OrderBy(f => f.Name)
                .ToListAsync();

            var result = fields.Select(f => new GetFieldDetail(
                f.Id, f.Name, f.AreaHa,
                f.keyPoints.Select(kp => new GeoPoint(kp.Keypoint.Y, kp.Keypoint.X)).ToList()
            ));

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetFieldDetail>> GetField(long id)
        {
            var field = await _context.Fields
                .AsNoTracking()
                .Include(f => f.keyPoints)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (field == null) return NotFound();

            return Ok(new GetFieldDetail(
                field.Id, field.Name, field.AreaHa,
                field.keyPoints.Select(kp => new GeoPoint(kp.Keypoint.Y, kp.Keypoint.X)).ToList()
            ));
        }

        [HttpPost("farm/{farmId}")]
        public async Task<ActionResult<GetFieldDetail>> CreateField(long farmId, [FromBody] CreateField dto)
        {
            var farm = await _context.Farms.FindAsync(farmId);
            if (farm == null) return NotFound("Farm not found");

            var field = new Models.Field.Field
            {
                Name = dto.Name.Trim(),
                AreaHa = dto.AreaHa,
                FarmId = farmId,
            };

            _context.Fields.Add(field);
            await _context.SaveChangesAsync();

            return Ok(new GetFieldDetail(field.Id, field.Name, field.AreaHa, new List<GeoPoint>()));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateField(long id, [FromBody] UpdateField dto)
        {
            var field = await _context.Fields.FindAsync(id);
            if (field == null) return NotFound();

            field.Name = dto.Name.Trim();
            field.AreaHa = dto.AreaHa;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}/keypoints")]
        public async Task<IActionResult> SaveKeypoints(long id, [FromBody] List<GeoPoint> points)
        {
            var field = await _context.Fields
                .Include(f => f.keyPoints)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (field == null) return NotFound();

            _context.FieldKeyPoints.RemoveRange(field.keyPoints);

            foreach (var p in points)
            {
                _context.FieldKeyPoints.Add(new Models.Field.FieldKeyPoint
                {
                    FieldId = id,
                    Keypoint = _geoFactory.CreatePoint(new Coordinate(p.Longitude, p.Latitude)),
                });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteField(long id)
        {
            var field = await _context.Fields.FindAsync(id);
            if (field == null) return NotFound();

            _context.Fields.Remove(field);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
