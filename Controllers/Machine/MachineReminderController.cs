using AGM_API.Database;
using AGM_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AGM_API.Controllers.Machine
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MachineReminderController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly FarmAuthorizationService _auth;
        private readonly ActivityLogService _activity;

        public MachineReminderController(AppDbContext context, FarmAuthorizationService auth, ActivityLogService activity)
        {
            _context = context;
            _auth = auth;
            _activity = activity;
        }

        [HttpGet("machine/{machineId}")]
        public async Task<ActionResult<IEnumerable<MachineReminderInfo>>> GetByMachine(long machineId)
        {
            var guardFarmId = await _auth.GetFarmIdForMachineAsync(machineId);
            if (guardFarmId == null) return NotFound();
            if (!await _auth.IsMemberAsync(guardFarmId.Value, User.GetUserId())) return Forbid();

            var entries = await _context.MachineReminders
                .AsNoTracking()
                .Where(r => r.MachineId == machineId && r.IsActive)
                .OrderBy(r => r.CreateDate)
                .Select(r => new MachineReminderInfo(
                    r.Id, r.MachineId, r.Machine.Name, r.Title, r.Notes,
                    r.DueDate, r.DueOperatingHours, r.RepeatIntervalDays, r.RepeatIntervalHours,
                    r.LastCompletedDate, r.IsActive
                ))
                .ToListAsync();
            return Ok(entries);
        }

        [HttpGet("farm/{farmId}")]
        public async Task<ActionResult<IEnumerable<MachineReminderInfo>>> GetByFarm(long farmId)
        {
            if (!await _auth.IsMemberAsync(farmId, User.GetUserId())) return Forbid();

            var entries = await _context.MachineReminders
                .AsNoTracking()
                .Where(r => r.Machine.FarmId == farmId && r.IsActive)
                .OrderBy(r => r.CreateDate)
                .Select(r => new MachineReminderInfo(
                    r.Id, r.MachineId, r.Machine.Name, r.Title, r.Notes,
                    r.DueDate, r.DueOperatingHours, r.RepeatIntervalDays, r.RepeatIntervalHours,
                    r.LastCompletedDate, r.IsActive
                ))
                .ToListAsync();
            return Ok(entries);
        }

        [HttpPost("machine/{machineId}")]
        public async Task<IActionResult> Create(long machineId, [FromBody] UpsertMachineReminder dto)
        {
            var guardFarmId = await _auth.GetFarmIdForMachineAsync(machineId);
            if (guardFarmId == null) return NotFound();
            if (!await _auth.CanWriteAsync(guardFarmId.Value, User.GetUserId())) return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title must not be empty");
            if (dto.DueDate == null && dto.DueOperatingHours == null)
                return BadRequest("Either DueDate or DueOperatingHours must be set");

            var machine = await _context.Machines.FindAsync(machineId);
            if (machine == null) return NotFound();

            var reminder = new Models.Machine.MachineReminder
            {
                MachineId = machineId,
                Title = dto.Title.Trim(),
                Notes = dto.Notes?.Trim(),
                DueDate = dto.DueDate,
                DueOperatingHours = dto.DueOperatingHours,
                RepeatIntervalDays = dto.RepeatIntervalDays,
                RepeatIntervalHours = dto.RepeatIntervalHours,
            };
            _context.MachineReminders.Add(reminder);
            await _context.SaveChangesAsync();

            await _activity.LogAsync(guardFarmId.Value, "MachineReminder", reminder.Id, "Created",
                $"{reminder.Title} · {machine.Name}");
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpsertMachineReminder dto)
        {
            var guardFarmId = await _auth.GetFarmIdForMachineReminderAsync(id);
            if (guardFarmId == null) return NotFound();
            if (!await _auth.CanWriteAsync(guardFarmId.Value, User.GetUserId())) return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest("Title must not be empty");
            if (dto.DueDate == null && dto.DueOperatingHours == null)
                return BadRequest("Either DueDate or DueOperatingHours must be set");

            var reminder = await _context.MachineReminders.Include(r => r.Machine).FirstOrDefaultAsync(r => r.Id == id);
            if (reminder == null) return NotFound();

            reminder.Title = dto.Title.Trim();
            reminder.Notes = dto.Notes?.Trim();
            reminder.DueDate = dto.DueDate;
            reminder.DueOperatingHours = dto.DueOperatingHours;
            reminder.RepeatIntervalDays = dto.RepeatIntervalDays;
            reminder.RepeatIntervalHours = dto.RepeatIntervalHours;
            await _context.SaveChangesAsync();

            await _activity.LogAsync(guardFarmId.Value, "MachineReminder", reminder.Id, "Updated",
                $"{reminder.Title} · {reminder.Machine.Name}");
            return NoContent();
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(long id)
        {
            var guardFarmId = await _auth.GetFarmIdForMachineReminderAsync(id);
            if (guardFarmId == null) return NotFound();
            if (!await _auth.CanWriteAsync(guardFarmId.Value, User.GetUserId())) return Forbid();

            var reminder = await _context.MachineReminders.Include(r => r.Machine).FirstOrDefaultAsync(r => r.Id == id);
            if (reminder == null) return NotFound();

            reminder.LastCompletedDate = DateTime.UtcNow;

            var repeats = false;
            if (reminder.RepeatIntervalDays.HasValue)
            {
                reminder.DueDate = DateTime.UtcNow.Date.AddDays(reminder.RepeatIntervalDays.Value);
                repeats = true;
            }
            if (reminder.RepeatIntervalHours.HasValue && reminder.Machine.OperatingHours.HasValue)
            {
                reminder.DueOperatingHours = reminder.Machine.OperatingHours.Value + reminder.RepeatIntervalHours.Value;
                repeats = true;
            }
            if (!repeats)
                reminder.IsActive = false;

            await _context.SaveChangesAsync();

            await _activity.LogAsync(guardFarmId.Value, "MachineReminder", reminder.Id, "Completed",
                $"{reminder.Title} · {reminder.Machine.Name}");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var guardFarmId = await _auth.GetFarmIdForMachineReminderAsync(id);
            if (guardFarmId == null) return NotFound();
            if (!await _auth.CanWriteAsync(guardFarmId.Value, User.GetUserId())) return Forbid();

            var reminder = await _context.MachineReminders.Include(r => r.Machine).FirstOrDefaultAsync(r => r.Id == id);
            if (reminder == null) return NotFound();

            var title = reminder.Title;
            var machineName = reminder.Machine.Name;

            _context.MachineReminders.Remove(reminder);
            await _context.SaveChangesAsync();

            await _activity.LogAsync(guardFarmId.Value, "MachineReminder", id, "Deleted",
                $"{title} · {machineName}");
            return NoContent();
        }
    }

    public record MachineReminderInfo(long Id, long MachineId, string MachineName, string Title, string? Notes,
        DateTime? DueDate, decimal? DueOperatingHours, int? RepeatIntervalDays, decimal? RepeatIntervalHours,
        DateTime? LastCompletedDate, bool IsActive);
    public record UpsertMachineReminder(string Title, string? Notes, DateTime? DueDate, decimal? DueOperatingHours,
        int? RepeatIntervalDays, decimal? RepeatIntervalHours);
}
