using AGM_API.Models.Audit;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AGM_API.Models.Field
{
    [Table("FieldAction")]
    public class FieldAction : Auditable
    {
        [Key, Required]
        public long Id { get; set; }
        public long FieldId { get; set; }
        public Field Field { get; set; } = null!;
        public DateTime Date { get; set; }
        public long ActionTypeId { get; set; }
        public FieldActionType ActionType { get; set; } = null!;
        public string? Notes { get; set; }
        public double? Amount { get; set; }
        public string? Unit { get; set; }
    }
}
