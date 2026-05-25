using AGM_API.Controllers.Records;
using AGM_API.Database;
using AGM_API.Models.Task;
using AGM_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGM_API.Controllers.Task
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ActivityLogService _activity;

        public TaskController(AppDbContext context, ActivityLogService activity)
        {
            _context = context;
            _activity = activity;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetTaskSimple>>> GetTasks(
              [FromQuery] long? seasonId,
              [FromQuery] long? assignedToId,
              [FromQuery] Models.Task.TaskStatus? status)
        {
            var query = _context.Tasks
                .AsNoTracking()
                .Include(t => t.Season)
                .Include(t => t.Fields).ThenInclude(tf => tf.Field)
                .Include(t => t.AssignedTo)
                .AsQueryable();

            if (seasonId.HasValue)
                query = query.Where(t => t.Season != null && t.Season.Id == seasonId.Value);

            if (assignedToId.HasValue)
                query = query.Where(t => t.AssignedTo != null && t.AssignedTo.Id == assignedToId.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            var tasks = await query
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            return Ok(tasks.Select(t => MapToSimple(t)));
        }

        [HttpGet("farm/{farmId}")]
        public async Task<ActionResult<IEnumerable<GetTaskSimple>>> GetTasksByFarm(
            long farmId,
            [FromQuery] bool overdueOnly = false)
        {
            var query = _context.Tasks
                .AsNoTracking()
                .Include(t => t.Season)
                .Include(t => t.Fields).ThenInclude(tf => tf.Field)
                .Include(t => t.AssignedTo)
                .Where(t => t.FarmId == farmId)
                .AsQueryable();

            if (overdueOnly)
                query = query.Where(t => t.DueDate <= DateTime.Today);

            var tasks = await query.OrderBy(t => t.DueDate).ToListAsync();

            return Ok(tasks.Select(t => MapToSimple(t)));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetTaskSimple>> GetTask(long id)
        {
            var task = await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Season)
                .Include(t => t.Fields).ThenInclude(tf => tf.Field)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            return Ok(MapToSimple(task));
        }

        [HttpPost]
        public async Task<ActionResult> CreateTask([FromBody] CreateTask dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Title is required");

            var season = dto.SeasonId.HasValue ? await _context.Seasons.FindAsync(dto.SeasonId.Value) : null;
            var assignedTo = dto.AssignedToId.HasValue ? await _context.Persons.FindAsync(dto.AssignedToId.Value) : null;

            var task = new Models.Task.Task
            {
                Name = dto.Name.Trim(),
                FarmId = dto.FarmId,
                Season = season,
                AssignedTo = assignedTo,
                DueDate = dto.DueDate,
                Status = dto.Status ?? Models.Task.TaskStatus.Pending,
                Priority = dto.Priority ?? Models.Task.TaskPriority.Medium
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            if (dto.FieldIds != null)
            {
                foreach (var fieldId in dto.FieldIds)
                    _context.TaskFields.Add(new TaskField { TaskId = task.Id, FieldId = fieldId });
                await _context.SaveChangesAsync();
            }

            await _activity.LogAsync(dto.FarmId, "Task", task.Id, "Created", task.Name);

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, new { task.Id, task.Name });
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult> PatchStatus(long id, [FromBody] PatchTaskStatus dto)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();
            task.Status = dto.Status;
            await _context.SaveChangesAsync();
            var statusLabel = dto.Status == Models.Task.TaskStatus.Completed ? "erledigt" : dto.Status.ToString().ToLower();
            await _activity.LogAsync(task.FarmId, "Task", id, "StatusChanged", $"{task.Name} → {statusLabel}");
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTask(long id, [FromBody] UpdateTask dto)
        {
            var task = await _context.Tasks
                .Include(t => t.Season)
                .Include(t => t.Fields)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();

            task.Name = dto.Name.Trim();
            task.DueDate = dto.DueDate;
            task.Status = dto.Status ?? task.Status;
            task.Priority = dto.Priority ?? task.Priority;
            task.Season = dto.SeasonId.HasValue ? await _context.Seasons.FindAsync(dto.SeasonId.Value) : null;
            task.AssignedTo = dto.AssignedToId.HasValue ? await _context.Persons.FindAsync(dto.AssignedToId.Value) : null;

            task.Fields.Clear();
            if (dto.FieldIds != null)
            {
                foreach (var fieldId in dto.FieldIds)
                    task.Fields.Add(new TaskField { TaskId = task.Id, FieldId = fieldId });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(long id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static GetTaskSimple MapToSimple(Models.Task.Task t) => new(
            t.Id,
            t.Name,
            t.Season != null ? new GetSeasonInfo(t.Season.Id, t.Season.Name) : null,
            t.Fields.Select(tf => new GetFieldInfo(tf.Field.Id, tf.Field.Name)).ToList(),
            t.AssignedTo != null ? new GetPersonInfo(t.AssignedTo.Id, t.AssignedTo.FirstName + " " + t.AssignedTo.Name) : null,
            t.DueDate,
            t.Status,
            t.Priority
        );
    }
}
