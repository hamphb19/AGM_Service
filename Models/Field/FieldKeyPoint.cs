using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Field
{
    [Table("FieldKeyPoint")]
    public class FieldKeyPoint
    {
        [Key, Required]
        public long Id { get; set; }
        [Required]
        public Point Keypoint { get; set; } = null!;
        public long FieldId { get; set; }
        [Required]
        public Field Field { get; set; } = null!;
    }
}
