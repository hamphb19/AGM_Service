using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AGM_API.Models.Audit;

namespace AGM_API.Models.Machine
{
    [Table("MachineReminder")]
    public class MachineReminder : Auditable
    {
        [Key, Required]
        public long Id { get; set; }
        public long MachineId { get; set; }
        public Machine Machine { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? DueOperatingHours { get; set; }
        public int? RepeatIntervalDays { get; set; }
        public decimal? RepeatIntervalHours { get; set; }
        public DateTime? LastCompletedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
