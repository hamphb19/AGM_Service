using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Animal
{
    [Table("StallAnimal")]
    public class StallAnimal
    {
        [Key, Required]
        public long Id { get; set; }
        public long StallId { get; set; }
        public Stall Stall { get; set; } = null!;
        public long AnimalTypeId { get; set; }
        public AnimalType AnimalType { get; set; } = null!;
        public int Count { get; set; }
        public string? Notes { get; set; }
    }
}
