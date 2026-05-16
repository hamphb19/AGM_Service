using AGM_API.Models.Audit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Animal
{
    [Table("Stall")]
    public class Stall : Auditable
    {
        [Key, Required]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public long FarmId { get; set; }
        public Farm.Farm Farm { get; set; } = null!;
        public long StallTypeId { get; set; }
        public StallType StallType { get; set; } = null!;
        public int Capacity { get; set; }
        public ICollection<StallAnimal> Animals { get; set; } = new List<StallAnimal>();
    }
}
