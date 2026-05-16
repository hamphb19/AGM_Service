using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Field
{
    [Table("FieldActionPhoto")]
    public class FieldActionPhoto
    {
        [Key, Required]
        public long Id { get; set; }
        public long FieldActionId { get; set; }
        public FieldAction FieldAction { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
