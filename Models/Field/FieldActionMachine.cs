using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Field
{
    [Table("FieldActionMachine")]
    public class FieldActionMachine
    {
        public long FieldActionId { get; set; }
        public FieldAction FieldAction { get; set; } = null!;
        public long MachineId { get; set; }
        public Machine.Machine Machine { get; set; } = null!;
    }
}
