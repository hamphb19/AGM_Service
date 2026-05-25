using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Task
{
    [Table("TaskField")]
    public class TaskField
    {
        public long TaskId { get; set; }
        public Task Task { get; set; } = null!;
        public long FieldId { get; set; }
        public Field.Field Field { get; set; } = null!;
    }
}
