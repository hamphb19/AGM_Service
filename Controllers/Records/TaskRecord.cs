using AGM_API.Models.Task;

namespace AGM_API.Controllers.Records
{
    public record GetTaskInfo(long Id, string ShortName, string Name, string Status, DateTime? DueDate);
    public record CreateTask(string ShortName, string Name, long? SeasonId, long? FieldId, long? AssignedToId, DateTime? DueDate, Models.Task.TaskStatus? Status, TaskPriority? Priority);
    public record PatchTaskStatus(Models.Task.TaskStatus Status);
    public record UpdateTask(string ShortName, string Name, long? FieldId, long? AssignedToId, DateTime? DueDate, Models.Task.TaskStatus? Status, TaskPriority? Priority);
    public record GetTaskSimple(long Id, string ShortName, string Name, GetSeasonInfo? Season, GetFieldInfo? Field, GetPersonInfo? Person, DateTime? DueDate, Models.Task.TaskStatus? Status, TaskPriority? Priority);
    
}
