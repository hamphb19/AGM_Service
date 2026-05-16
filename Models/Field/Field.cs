using AGM_API.Models.Audit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Field
{
    [Table("Field")]
    public class Field : Auditable
    {
        [Key, Required]
        public long Id { get; set; }
        [Required]
        public string Name {  get; set; } = null!;
        public double AreaHa {  get; set; }

        public long FarmId { get; set; }
        public Models.Farm.Farm Farm { get; set; } = null!;

        public ICollection<FieldKeyPoint> keyPoints { get; set; } = new List<FieldKeyPoint>();
    }
}
