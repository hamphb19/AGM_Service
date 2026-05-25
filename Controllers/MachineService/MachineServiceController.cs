using AGM_API.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Controllers.MachineService
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MachineServiceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MachineServiceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("machine/{machineId}")]
        public async Task<ActionResult<IEnumerable<MachineServiceInfo>>> GetByMachine(long machineId)
        {
            var entries = await _context.MachineServices
                .AsNoTracking()
                .Where(s => s.MachineId == machineId)
                .OrderByDescending(s => s.Date)
                .Select(s => new MachineServiceInfo(
                    s.Id, s.Date, (int)s.ServiceType,
                    s.Odometer, s.OperatingHours, s.Cost, s.Notes, s.NextServiceDate
                ))
                .ToListAsync();
            return Ok(entries);
        }

        [HttpPost("machine/{machineId}")]
        public async Task<IActionResult> Create(long machineId, [FromBody] UpsertMachineService dto)
        {
            var machine = await _context.Machines.FindAsync(machineId);
            if (machine == null) return NotFound();

            _context.MachineServices.Add(new Models.Machine.MachineService
            {
                MachineId = machineId,
                Date = dto.Date,
                ServiceType = (Models.Machine.MachineServiceType)dto.ServiceType,
                Odometer = dto.Odometer,
                OperatingHours = dto.OperatingHours,
                Cost = dto.Cost,
                Notes = dto.Notes?.Trim(),
                NextServiceDate = dto.NextServiceDate,
            });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var entry = await _context.MachineServices.FindAsync(id);
            if (entry == null) return NotFound();
            _context.MachineServices.Remove(entry);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public record MachineServiceInfo(long Id, DateTime Date, int ServiceType, int? Odometer, decimal? OperatingHours, decimal? Cost, string? Notes, DateTime? NextServiceDate);
    public record UpsertMachineService(DateTime Date, int ServiceType, int? Odometer, decimal? OperatingHours, decimal? Cost, string? Notes, DateTime? NextServiceDate);
}
