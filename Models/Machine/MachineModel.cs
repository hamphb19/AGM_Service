using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Machine
{
    [Table("MachineModel")]
    public class MachineModel
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public long MachineTypeId { get; set; }
        public MachineType MachineType { get; set; } = null!;
        public long? MachineBrandId { get; set; }
        public MachineBrand? MachineBrand { get; set; }
    }
}
