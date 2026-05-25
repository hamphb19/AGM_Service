using AGM_API.Models.Task;

namespace AGM_API.Controllers.Records
{
    public record GetTaskInfo(long Id, string Name, string Status, DateTime? DueDate);
    public record CreateTask(string Name, long FarmId, long? SeasonId, List<long>? FieldIds, long? AssignedToId, DateTime? DueDate, Models.Task.TaskStatus? Status, TaskPriority? Priority);
    public record PatchTaskStatus(Models.Task.TaskStatus Status);
    public record UpdateTask(string Name, long? SeasonId, List<long>? FieldIds, long? AssignedToId, DateTime? DueDate, Models.Task.TaskStatus? Status, TaskPriority? Priority);
    public record GetTaskSimple(long Id, string Name, GetSeasonInfo? Season, List<GetFieldInfo> Fields, GetPersonInfo? Person, DateTime? DueDate, Models.Task.TaskStatus? Status, TaskPriority? Priority);
}
