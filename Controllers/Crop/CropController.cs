using AGM_API.Controllers.Records;
using AGM_API.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Controllers.Crop
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CropController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CropController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetCropSimple>>> GetCrops()
        {
            var crops = await _context.Crops
                .AsNoTracking()
                .Include(c => c.CropType)
                .OrderBy(c => c.CropType.Name)
                .ThenBy(c => c.Name)
                .Select(c => new GetCropSimple(
                    c.Id,
                    c.Name,
                    c.ShortName,
                    new GetCropTypeInfo(c.CropType.Id, c.CropType.Name, c.CropType.ShortName)
                ))
                .ToListAsync();

            return Ok(crops);
        }
    }
}
