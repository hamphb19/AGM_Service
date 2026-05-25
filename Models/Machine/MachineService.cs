using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AGM_API.Models.Audit;

namespace AGM_API.Models.Machine
{
    [Table("MachineService")]
    public class MachineService : Auditable
    {
        [Key, Required]
        public long Id { get; set; }
        public long MachineId { get; set; }
        public Machine Machine { get; set; } = null!;
        public DateTime Date { get; set; }
        public MachineServiceType ServiceType { get; set; }
        public int? Odometer { get; set; }
        public decimal? OperatingHours { get; set; }
        public decimal? Cost { get; set; }
        public string? Notes { get; set; }
        public DateTime? NextServiceDate { get; set; }
    }

    public enum MachineServiceType
    {
        OilChange = 0,
        Inspection = 1,
        Repair = 2,
        Tires = 3,
        Other = 4,
    }
}
