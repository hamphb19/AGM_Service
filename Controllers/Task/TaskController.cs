using AGM_API.Controllers.Records;
using AGM_API.Database;
using AGM_API.Models.Task;
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

        public TaskController(AppDbContext context)
        {
            _context = context;
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
                .Include(t => t.Field)
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
                .Select(t => new GetTaskSimple(
                    t.Id,
                    t.ShortName,
                    t.Name,
                    t.Season != null ? new GetSeasonInfo(t.Season.Id, t.Season.Name) : null,
                    t.Field != null ? new GetFieldInfo(t.Field.Id, t.Field.Name) : null,
                    t.AssignedTo != null ? new GetPersonInfo(t.AssignedTo.Id, t.AssignedTo.FirstName + " " + t.AssignedTo.Name) : null,
                    t.DueDate,
                    t.Status,
                    t.Priority
                ))
                .ToListAsync();

            return Ok(tasks);
        }
        [HttpGet("farm/{farmId}")]
        public async Task<ActionResult<IEnumerable<GetTaskSimple>>> GetTasksByFarm(
            long farmId,
            [FromQuery] bool overdueOnly = false)
        {
            var query = _context.Tasks
                .AsNoTracking()
                .Include(t => t.Season)
                    .ThenInclude(s => s!.Farm)
                .Include(t => t.Field)
                .Include(t => t.AssignedTo)
                .Where(t => t.Season != null && t.Season.Farm.Id == farmId)
                .AsQueryable();

            if (overdueOnly)
                query = query.Where(t => t.DueDate <= DateTime.Today);

            var tasks = await query
                .OrderBy(t => t.DueDate)
                .Select(t => new GetTaskSimple(
                    t.Id,
                    t.ShortName,
                    t.Name,
                    t.Season != null ? new GetSeasonInfo(t.Season.Id, t.Season.Name) : null,
                    t.Field != null ? new GetFieldInfo(t.Field.Id, t.Field.Name) : null,
                    t.AssignedTo != null ? new GetPersonInfo(t.AssignedTo.Id, t.AssignedTo.FirstName + " " + t.AssignedTo.Name) : null,
                    t.DueDate,
                    t.Status,
                    t.Priority
                ))
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetTaskSimple>> GetTask(long id)
        {
            var task = await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Season)
                .Include(t => t.Field)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
                return NotFound();

            return Ok(new GetTaskSimple(
                task.Id,
                task.ShortName,
                task.Name,
                task.Season != null ? new GetSeasonInfo(task.Season.Id, task.Season.Name) : null,
                task.Field != null ? new GetFieldInfo(task.Field.Id, task.Field.Name) : null,
                task.AssignedTo != null ? new GetPersonInfo(task.AssignedTo.Id, task.AssignedTo.FirstName + " " + task.AssignedTo.Name) : null,
                task.DueDate,
                task.Status,
                task.Priority
            ));
        }

        [HttpPost]
        public async Task<ActionResult> CreateTask([FromBody] CreateTask dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Title is required");

            var season = dto.SeasonId.HasValue ? await _context.Seasons.FindAsync(dto.SeasonId.Value) : null;
            var field = dto.FieldId.HasValue ? await _context.Fields.FindAsync(dto.FieldId.Value) : null;
            var assignedTo = dto.AssignedToId.HasValue ? await _context.Persons.FindAsync(dto.AssignedToId.Value) : null;

            var task = new Models.Task.Task
            {
                Name = dto.Name.Trim(),
                ShortName = dto.ShortName.Trim(),
                Season = season,
                Field = field,
                AssignedTo = assignedTo,
                DueDate = dto.DueDate,
                Status = dto.Status ?? Models.Task.TaskStatus.Pending,
                Priority = dto.Priority ?? Models.Task.TaskPriority.Medium
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, new { task.Id, task.Name });
        }

        [HttpPatch("{id}/status")]
        public async Task<ActionResult> PatchStatus(long id, [FromBody] PatchTaskStatus dto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();
            task.Status = dto.Status;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateTask(long id, [FromBody] UpdateTask dto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            task.Name = dto.Name.Trim();
            task.ShortName = dto.ShortName.Trim();
            task.DueDate = dto.DueDate;
            task.Status = dto.Status ?? task.Status;
            task.Priority = dto.Priority ?? task.Priority;

            if (dto.FieldId.HasValue)
                task.Field = await _context.Fields.FindAsync(dto.FieldId.Value);
            if (dto.AssignedToId.HasValue)
                task.AssignedTo = await _context.Persons.FindAsync(dto.AssignedToId.Value);

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
    }
}
