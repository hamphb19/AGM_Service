using AGM_API.Models.Audit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Task
{
    [Table("Task")]
    public class Task : Auditable
    {
        [Key, Required]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; }
        public long FarmId { get; set; }
        public Farm.Farm? Farm { get; set; }
        public Season.Season? Season { get; set; }
        public ICollection<TaskField> Fields { get; set; } = new List<TaskField>();
        public Person.Person? AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }

        [Required]
        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    }
    public enum TaskStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }
    public enum TaskPriority
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Urgent = 3
    }
}
