using AGM_API.Models.Audit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Animal
{
    [Table("Animal")]
    public class Animal : Auditable
    {
        [Key, Required]
        public long Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;    
        [Required]
        public string ShortName { get; set; } = null!;
        [Required]
        public AnimalType AnimalType { get; set; } = null!;

    }
}
