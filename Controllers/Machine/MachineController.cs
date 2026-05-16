using AGM_API.Database;
using AGM_API.Models.Machine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Controllers.Machine
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MachineController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MachineController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<MachineTypeInfo>>> GetTypes()
        {
            var types = await _context.MachineTypes
                .AsNoTracking()
                .OrderBy(t => t.Id)
                .Select(t => new MachineTypeInfo(t.Id, t.Name, t.ShortName))
                .ToListAsync();
            return Ok(types);
        }

        [HttpGet("brands")]
        public async Task<ActionResult<IEnumerable<MachineBrandInfo>>> GetBrands()
        {
            var brands = await _context.MachineBrands
                .AsNoTracking()
                .OrderBy(b => b.Name)
                .Select(b => new MachineBrandInfo(b.Id, b.Name, b.ShortName))
                .ToListAsync();
            return Ok(brands);
        }

        [HttpGet("farm/{farmId}")]
        public async Task<ActionResult<IEnumerable<MachineInfo>>> GetMachines(long farmId)
        {
            var machines = await _context.Machines
                .AsNoTracking()
                .Where(m => m.FarmId == farmId)
                .Include(m => m.MachineType)
                .Include(m => m.MachineBrand)
                .OrderBy(m => m.Name)
                .Select(m => new MachineInfo(
                    m.Id,
                    m.Name,
                    new MachineTypeInfo(m.MachineType.Id, m.MachineType.Name, m.MachineType.ShortName),
                    m.MachineBrand != null ? new MachineBrandInfo(m.MachineBrand.Id, m.MachineBrand.Name, m.MachineBrand.ShortName) : null,
                    m.YearOfManufacture,
                    m.LicensePlate,
                    m.Notes,
                    (int)m.Status
                ))
                .ToListAsync();
            return Ok(machines);
        }

        [HttpPost("farm/{farmId}")]
        public async Task<IActionResult> CreateMachine(long farmId, [FromBody] UpsertMachine dto)
        {
            var farm = await _context.Farms.FindAsync(farmId);
            if (farm == null) return NotFound("Farm not found");

            var machineType = await _context.MachineTypes.FirstOrDefaultAsync(t => t.ShortName == dto.MachineTypeShortName);
            if (machineType == null) return BadRequest("Machine type not found");

            long? brandId = null;
            if (!string.IsNullOrEmpty(dto.MachineBrandShortName))
            {
                var brand = await _context.MachineBrands.FirstOrDefaultAsync(b => b.ShortName == dto.MachineBrandShortName);
                brandId = brand?.Id;
            }

            _context.Machines.Add(new Models.Machine.Machine
            {
                Name = dto.Name.Trim(),
                FarmId = farmId,
                MachineTypeId = machineType.Id,
                MachineBrandId = brandId,
                YearOfManufacture = dto.YearOfManufacture,
                LicensePlate = dto.LicensePlate?.Trim(),
                Notes = dto.Notes?.Trim(),
                Status = (MachineStatus)dto.Status,
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMachine(long id, [FromBody] UpsertMachine dto)
        {
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null) return NotFound();

            var machineType = await _context.MachineTypes.FirstOrDefaultAsync(t => t.ShortName == dto.MachineTypeShortName);
            if (machineType == null) return BadRequest("Machine type not found");

            long? brandId = null;
            if (!string.IsNullOrEmpty(dto.MachineBrandShortName))
            {
                var brand = await _context.MachineBrands.FirstOrDefaultAsync(b => b.ShortName == dto.MachineBrandShortName);
                brandId = brand?.Id;
            }

            machine.Name = dto.Name.Trim();
            machine.MachineTypeId = machineType.Id;
            machine.MachineBrandId = brandId;
            machine.YearOfManufacture = dto.YearOfManufacture;
            machine.LicensePlate = dto.LicensePlate?.Trim();
            machine.Notes = dto.Notes?.Trim();
            machine.Status = (MachineStatus)dto.Status;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMachine(long id)
        {
            var machine = await _context.Machines.FindAsync(id);
            if (machine == null) return NotFound();
            _context.Machines.Remove(machine);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public record MachineTypeInfo(long Id, string Name, string ShortName);
    public record MachineBrandInfo(long Id, string Name, string ShortName);
    public record MachineInfo(long Id, string Name, MachineTypeInfo MachineType, MachineBrandInfo? MachineBrand, int? YearOfManufacture, string? LicensePlate, string? Notes, int Status);
    public record UpsertMachine(string Name, string MachineTypeShortName, string? MachineBrandShortName, int? YearOfManufacture, string? LicensePlate, string? Notes, int Status);
}
