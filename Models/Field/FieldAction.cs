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

        // AMA documentation fields
        public string? Product { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Pest { get; set; }
        public double? Temperature { get; set; }
        public double? WindSpeed { get; set; }
        public string? Applicator { get; set; }
        public double? NContent { get; set; }
        public int? FertilizerType { get; set; }
        public string? Variety { get; set; }

        public long? CropId { get; set; }
        public Models.Crop.Crop? Crop { get; set; }

        public long? SeasonId { get; set; }
        public Season.Season? Season { get; set; }

        public ICollection<FieldActionMachine> Machines { get; set; } = new List<FieldActionMachine>();
    }
}
