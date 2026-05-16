using AGM_API.Models.Audit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Machine
{
    [Table("Machine")]
    public class Machine : Auditable
    {
        [Key, Required]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public long FarmId { get; set; }
        public Farm.Farm Farm { get; set; } = null!;
        public long MachineTypeId { get; set; }
        public MachineType MachineType { get; set; } = null!;
        public long? MachineBrandId { get; set; }
        public MachineBrand? MachineBrand { get; set; }
        public int? YearOfManufacture { get; set; }
        public string? LicensePlate { get; set; }
        public string? Notes { get; set; }
        public MachineStatus Status { get; set; } = MachineStatus.Available;
    }

    public enum MachineStatus
    {
        Available = 0,
        Maintenance = 1,
        Broken = 2,
    }
}
